document.addEventListener("DOMContentLoaded", function () {
    // Dark / Light mode toggle (Sun & Moon)
    const themeToggleBtn = document.getElementById("themeToggleBtn");
    if (themeToggleBtn) {
        themeToggleBtn.addEventListener("click", function () {
            const currentTheme = document.documentElement.getAttribute("data-theme") || "light";
            const nextTheme = currentTheme === "dark" ? "light" : "dark";
            document.documentElement.setAttribute("data-theme", nextTheme);
            localStorage.setItem("ch-theme", nextTheme);
        });
    }

    // Confirmation modal for destructive actions and logout
    const confirmForms = document.querySelectorAll("form[data-confirm]");
    const modalEl = document.getElementById("chConfirmModal");
    let pendingForm = null;

    if (modalEl && typeof bootstrap !== "undefined") {
        const bsModal = new bootstrap.Modal(modalEl);
        const messageEl = document.getElementById("chConfirmModalMessage");
        const confirmBtn = document.getElementById("chConfirmModalSubmit");

        confirmForms.forEach(function (form) {
            form.addEventListener("submit", function (e) {
                if (form.dataset.confirmed === "true") {
                    return;
                }
                e.preventDefault();
                pendingForm = form;
                if (messageEl) {
                    messageEl.textContent = form.getAttribute("data-confirm") || "Вы уверены, что хотите выполнить это действие?";
                }
                bsModal.show();
            });
        });

        if (confirmBtn) {
            confirmBtn.addEventListener("click", function () {
                if (pendingForm) {
                    pendingForm.dataset.confirmed = "true";
                    confirmBtn.disabled = true;
                    confirmBtn.textContent = "Выполняется...";
                    pendingForm.submit();
                }
            });
        }
    }

    // Loading state on form submission
    const allForms = document.querySelectorAll("form:not([data-confirm]):not([method='get'])");
    allForms.forEach(function (form) {
        form.addEventListener("submit", function () {
            const submitBtn = form.querySelector("button[type='submit']");
            if (submitBtn && !submitBtn.dataset.noLoading) {
                setTimeout(function () {
                    if (!form.querySelector(".input-validation-error")) {
                        submitBtn.disabled = true;
                        const originalText = submitBtn.innerHTML;
                        submitBtn.dataset.originalText = originalText;
                        submitBtn.innerHTML = '<span class="spinner-border spinner-border-sm me-2" role="status" aria-hidden="true"></span>Подождите...';
                    }
                }, 20);
            }
        });
    });
});
