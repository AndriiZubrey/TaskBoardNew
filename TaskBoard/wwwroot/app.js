const tasksEl = document.getElementById("tasks");
const summaryEl = document.getElementById("summary");
const form = document.getElementById("taskForm");
const formError = document.getElementById("formError");

async function loadData() {
    const [tasksResponse, summaryResponse] = await Promise.all([
        fetch("/api/tasks"),
        fetch("/api/tasks/summary")
    ]);

    const tasks = await tasksResponse.json();
    const summary = await summaryResponse.json();

    renderSummary(summary);
    renderTasks(tasks);
}

function renderSummary(summary) {
    summaryEl.innerHTML = `
    <div><strong>Всього:</strong> ${summary.total}</div>
    <div><strong>Виконано:</strong> ${summary.completed}</div>
    <div><strong>Активні:</strong> ${summary.pending}</div>
    <div><strong>Прострочені:</strong> ${summary.overdue}</div>
    <div><strong>Середній пріоритет:</strong> ${summary.averagePriority}</div>
  `;
}

function renderTasks(tasks) {
    if (!tasks.length) {
        tasksEl.innerHTML = "<p>Немає задач.</p>";
        return;
    }

    tasksEl.innerHTML = `
    <table>
      <thead>
        <tr>
          <th>Назва</th>
          <th>Дедлайн</th>
          <th>Пріоритет</th>
          <th>Статус</th>
          <th></th>
        </tr>
      </thead>
      <tbody>
        ${tasks.map(t => `
          <tr>
            <td>
              <strong>${escapeHtml(t.title)}</strong><br />
              <small>${escapeHtml(t.description ?? "")}</small>
            </td>
            <td>${t.dueDate}</td>
            <td>${t.priority}</td>
            <td>${t.isCompleted ? "Виконано" : "Активна"}</td>
            <td>
              ${t.isCompleted ? "" : `<button data-id="${t.id}" class="completeBtn">Готово</button>`}
            </td>
          </tr>
        `).join("")}
      </tbody>
    </table>
  `;

    document.querySelectorAll(".completeBtn").forEach(btn => {
        btn.addEventListener("click", async () => {
            const id = btn.getAttribute("data-id");
            await fetch(`/api/tasks/${id}/complete`, { method: "POST" });
            await loadData();
        });
    });
}

form.addEventListener("submit", async (e) => {
    e.preventDefault();
    formError.textContent = "";

    const data = new FormData(form);
    const payload = {
        title: data.get("title"),
        description: data.get("description"),
        dueDate: data.get("dueDate"),
        priority: Number(data.get("priority"))
    };

    const response = await fetch("/api/tasks", {
        method: "POST",
        headers: { "Content-Type": "application/json" },
        body: JSON.stringify(payload)
    });

    if (!response.ok) {
        const error = await response.json();
        formError.textContent = error?.errors?.request?.[0] ?? "Помилка створення задачі.";
        return;
    }

    form.reset();
    await loadData();
});

function escapeHtml(value) {
    return String(value)
        .replaceAll("&", "&amp;")
        .replaceAll("<", "&lt;")
        .replaceAll(">", "&gt;")
        .replaceAll('"', "&quot;")
        .replaceAll("'", "&#39;");
}

loadData();