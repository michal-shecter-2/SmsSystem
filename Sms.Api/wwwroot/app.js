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

const employeeSelect = document.getElementById("employee");
const sendForm = document.getElementById("send-form");
const phoneInput = document.getElementById("phone");
const bodyInput = document.getElementById("body");
const sendButton = document.getElementById("send-button");
let sending = false;

function updateSendButton() {
    sendButton.disabled = sending || phoneInput.disabled || !phoneInput.value;
}

async function loadEmployees() {
    try {
        const response = await fetch("/employees");

        if (!response.ok) {
            throw new Error("Employee loading failed.");
        }

        const employees = await response.json();
        employeeSelect.replaceChildren(new Option(
            employees.length ? "בחרי עובד" : "אין עובדים להצגה", ""));

        for (const employee of employees) {
            employeeSelect.add(new Option(
                `${employee.firstName} ${employee.lastName} — ${employee.address}`,
                employee.id));
        }

        employeeSelect.disabled = employees.length === 0;
    } catch (error) {
        employeeSelect.replaceChildren(new Option("טעינת העובדים נכשלה", ""));
        notice.textContent = "לא ניתן לטעון עובדים. נסי לרענן את הדף.";
    }
}

async function loadPhones() {
    const employeeId = employeeSelect.value;
    phoneInput.disabled = true;
    phoneInput.replaceChildren(new Option("בחרי מספר טלפון", ""));
    updateSendButton();

    if (!employeeId) {
        return;
    }

    try {
        const response = await fetch(`/employees/${employeeId}/phones`);

        if (!response.ok) {
            throw new Error("Phone loading failed.");
        }

        const phones = await response.json();

        // Ignore a response for an employee who is no longer selected.
        if (employeeSelect.value !== employeeId) {
            return;
        }

        for (const phone of phones) {
            phoneInput.add(new Option(phone, phone));
        }

        phoneInput.disabled = phones.length === 0;

        if (phones.length === 0) {
            phoneInput.replaceChildren(new Option("אין מספרי טלפון לעובד", ""));
        }
    } catch (error) {
        if (employeeSelect.value === employeeId) {
            notice.textContent = "טעינת מספרי הטלפון נכשלה. בחרי שוב את העובד.";
        }
    }

    updateSendButton();
}

employeeSelect.addEventListener("change", loadPhones);
phoneInput.addEventListener("change", updateSendButton);

sendForm.addEventListener("submit", async (event) => {
    event.preventDefault();

    if (sendButton.disabled) {
        return;
    }

    if (!bodyInput.value.trim()) {
        notice.textContent = "יש להזין תוכן שאינו ריק.";
        return;
    }

    sending = true;
    updateSendButton();
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
        sending = false;
        updateSendButton();
        sendButton.textContent = "שליחה";
    }
});

updateSendButton();
loadEmployees();