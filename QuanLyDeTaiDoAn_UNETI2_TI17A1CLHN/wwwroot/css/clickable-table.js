
document.addEventListener("DOMContentLoaded", function () {
    document.querySelectorAll(".clickable-row").forEach(function (row) {
        function openDetails() {
            const url = row.dataset.url;
            if (url) {
                window.location.href = url;
            }
        }

        row.addEventListener("click", function (event) {
            if (event.target.closest(
                "a, button, input, select, textarea, form"
            )) {
                return;
            }

            openDetails();
        });

        row.addEventListener("keydown", function (event) {
            if (event.target !== row) return;

            if (event.key === "Enter" || event.key === " ") {
                event.preventDefault();
                openDetails();
            }
        });
    });
});
