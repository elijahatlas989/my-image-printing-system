// Display local JPEG previews before the customer uploads the files.
document.addEventListener("DOMContentLoaded", function () {
    const input = document.getElementById("photoInput");
    const preview = document.getElementById("photoPreview");

    if (input && preview) {
        input.addEventListener("change", function () {
            preview.innerHTML = "";

            Array.from(input.files || []).forEach(function (file) {
                if (file.type === "image/jpeg") {
                    const image = document.createElement("img");
                    image.src = URL.createObjectURL(file);
                    image.alt = file.name;
                    preview.appendChild(image);
                }
            });
        });
    }

    // Hide the card number box when direct payment is selected.
    const paymentMethod = document.getElementById("paymentMethod");
    const cardSection = document.getElementById("cardSection");

    if (paymentMethod && cardSection) {
        const updateCardVisibility = function () {
            cardSection.style.display = paymentMethod.value === "Credit Card" ? "block" : "none";
        };

        paymentMethod.addEventListener("change", updateCardVisibility);
        updateCardVisibility();
    }
});
