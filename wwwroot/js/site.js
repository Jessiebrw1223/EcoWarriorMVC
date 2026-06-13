console.log("EcoWarrior MVC listo.");

document.addEventListener("DOMContentLoaded", () => {
    configurarBotonesRetos();
    configurarCompartir();
    configurarMapa();
});

function configurarBotonesRetos() {
    const botones = document.querySelectorAll(".reto-footer button, .challenge-card button");

    botones.forEach((boton) => {
        boton.addEventListener("click", () => {
            boton.disabled = true;
            boton.textContent = "Completado";

            const tarjeta = boton.closest(".reto-card, .challenge-card");
            if (tarjeta) {
                tarjeta.classList.add("reto-completado");
            }

            alert("Reto registrado correctamente. Tus puntos se actualizarán en el perfil.");
        });
    });
}

function configurarCompartir() {
    const botonCompartir = document.querySelector(".share-btn");

    if (!botonCompartir) return;

    botonCompartir.addEventListener("click", async () => {
        const texto = "Estoy avanzando como EcoWarrior y reduciendo mi impacto ambiental 🌱";

        if (navigator.share) {
            await navigator.share({
                title: "EcoWarrior",
                text: texto,
                url: window.location.href
            });
        } else {
            await navigator.clipboard.writeText(texto);
            alert("Estadísticas copiadas al portapapeles.");
        }
    });
}

function configurarMapa() {
    const botonesUnirse = document.querySelectorAll(".map-footer button");

    botonesUnirse.forEach((boton) => {
        boton.addEventListener("click", () => {
            boton.disabled = true;
            boton.textContent = "Unido";
            alert("Te uniste al encuentro ecológico.");
        });
    });
}