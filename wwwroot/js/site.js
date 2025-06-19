document.addEventListener("DOMContentLoaded", function () {
    const userToggle = document.getElementById("userToggle");
    const dropdown = document.getElementById("userDropdown");
    const overlay = document.getElementById("dropdownOverlay");

    if (userToggle && dropdown && overlay) {
        userToggle.addEventListener("click", function (e) {
            const isActive = dropdown.classList.toggle("active");
            overlay.style.display = isActive ? "block" : "none";
            document.body.classList.toggle("blur-active", isActive);
        });

        overlay.addEventListener("click", function () {
            dropdown.classList.remove("active");
            overlay.style.display = "none";
            document.body.classList.remove("blur-active");
        });

        document.addEventListener("keydown", function (e) {
            if (e.key === "Escape") {
                dropdown.classList.remove("active");
                overlay.style.display = "none";
                document.body.classList.remove("blur-active");
            }
        });
    }
});
document.addEventListener("DOMContentLoaded", function () {
    const avatarInput = document.getElementById("avatarInput");
    const avatarPreview = document.getElementById("avatarPreview");
    const avatarForm = document.getElementById("avatarForm");

    if (avatarInput && avatarPreview && avatarForm) {
        avatarInput.addEventListener("change", function () {
            const file = avatarInput.files[0];
            if (file) {
                const reader = new FileReader();
                reader.onload = function (e) {
                    avatarPreview.src = e.target.result;
                };
                reader.readAsDataURL(file);

                // Tự động gửi form
                setTimeout(() => avatarForm.submit(), 500);
            }
        });
    }
});
// Tab switching
document.querySelectorAll(".profile-menu li").forEach(item => {
    item.addEventListener("click", function () {
        document.querySelectorAll(".profile-menu li").forEach(li => li.classList.remove("active"));
        document.querySelectorAll(".profile-section").forEach(section => section.classList.remove("active"));

        this.classList.add("active");
        const targetId = this.getAttribute("data-target");
        document.getElementById(targetId).classList.add("active");
    });
});
