const tableBody = document.getElementById("messages");
const statusFilter = document.getElementById("status-filter");
const refreshButton = document.getElementById("refresh-button");
const notice = document.getElementById("notice");

async function loadMessages() {
    try {
        const status = statusFilter.value;

        const url = status
            ? `/messages?status=${encodeURIComponent(status)}`
            : "/messages";

        const response = await fetch(url);

        if (!response.ok) {
            throw new Error("טעינת ההודעות נכשלה.");
        }

        const messages = await response.json();

        tableBody.replaceChildren();

        for (const message of messages) {
            const row = document.createElement("tr");

            const values = [
                message.to,
                message.body,
                message.status,
                new Date(message.createdAt).toLocaleString("he-IL")
            ];

            for (const value of values) {
                const cell = document.createElement("td");
                cell.textContent = value;
                row.appendChild(cell);
            }

            const actionCell = document.createElement("td");
            const button = document.createElement("button");

            button.type = "button";
            button.textContent = "עדכון סטטוס";
            button.disabled = message.status !== "accepted";

            button.addEventListener("click", () =>
                checkStatus(message.id, button));

            actionCell.appendChild(button);
            row.appendChild(actionCell);
            tableBody.appendChild(row);
        }

        if (messages.length === 0) {
            const row = document.createElement("tr");
            const cell = document.createElement("td");

            cell.colSpan = 5;
            cell.textContent = "אין הודעות להצגה.";

            row.appendChild(cell);
            tableBody.appendChild(row);
        }
    } catch (error) {
        notice.textContent = "לא ניתן לטעון את ההודעות. נסי לרענן.";
    }
}

async function checkStatus(id, button) {
    button.disabled = true;

    try {
        const response = await fetch(`/messages/${id}`);

        if (!response.ok) {
            throw new Error("בדיקת הסטטוס נכשלה.");
        }

        const message = await response.json();

        notice.textContent = message.error
            ? "לא ניתן לרענן את הסטטוס מול הספק. מוצג המצב האחרון שנשמר."
            : `סטטוס ההודעה: ${message.status}`;

        await loadMessages();
    } catch (error) {
        notice.textContent = "בדיקת הסטטוס נכשלה. אפשר לנסות שוב.";
    } finally {
        button.disabled = false;
    }
}

statusFilter.disabled = false;
refreshButton.disabled = false;

statusFilter.addEventListener("change", loadMessages);
refreshButton.addEventListener("click", loadMessages);

loadMessages();

const sendForm = document.getElementById("send-form");
const phoneInput = document.getElementById("phone");
const bodyInput = document.getElementById("body");
const sendButton = document.getElementById("send-button");

sendForm.addEventListener("submit", async (event) => {
    event.preventDefault();

    if (sendButton.disabled) {
        return;
    }

    if (!bodyInput.value.trim()) {
        notice.textContent = "יש להזין תוכן שאינו ריק.";
        return;
    }

    sendButton.disabled = true;
    sendButton.textContent = "שולח…";
    notice.textContent = "";

    try {
        const response = await fetch("/messages", {
            method: "POST",
            headers: {
                "Content-Type": "application/json"
            },
            body: JSON.stringify({
                to: phoneInput.value.trim(),
                body: bodyInput.value
            })
        });

        if (response.status === 400) {
            notice.textContent =
                "הקלט אינו תקין. בדקי את מספר הטלפון ואת אורך ההודעה.";
            return;
        }

        if (!response.ok) {
            throw new Error("Message creation was not confirmed.");
        }

        const message = await response.json();

        notice.textContent =
            `נוצרה הודעה: ${message.id} | סטטוס: ${message.status}`;

        bodyInput.value = "";

        await loadMessages();
    } catch (error) {
        notice.textContent =
            "לא התקבל אישור ליצירת ההודעה. בדקי ברשימה לפני ניסיון נוסף, כדי להימנע מכפילות.";
    } finally {
        sendButton.disabled = false;
        sendButton.textContent = "שליחה";
    }
});

sendButton.disabled = false;