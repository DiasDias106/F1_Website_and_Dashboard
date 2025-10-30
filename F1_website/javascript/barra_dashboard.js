document.addEventListener("DOMContentLoaded", () => {
    const toggleBtn = document.getElementById("btn");
    const sidebar = document.getElementById("sidebar");
    const homeContent = document.querySelector(".home-content");
    const modeToggle = document.getElementById("mode-toggle");
    const body = document.body;

    toggleBtn.onclick = () => {
        sidebar.classList.toggle("close");
        if (sidebar.classList.contains("close")) {
            homeContent.style.marginLeft = "80px";
        } else {
            homeContent.style.marginLeft = "250px";
        }
    }

    modeToggle.onchange = () => {
        body.classList.toggle("dark-mode");
        body.classList.toggle("light-mode");
    }
});