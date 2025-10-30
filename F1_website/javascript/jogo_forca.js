const palavras = ["HAMILTON", "FERRARI", "PITSTOP", "VERSTAPEN", "PNEUS", "LECLERC", "CONSTRUTORES", "NORRIS", "VITORIA", "SAFETYCAR", "AERODINAMICA", "SENNA", "PILOTOS", "MOTORELETRICO"];

let palavraEscolhida = "";
let letrasCertas = [];
let letrasErradas = [];
let ultimaLetra = "";
const maxTentativas = 6;

const palavraEl = document.querySelector(".word");
const letrasEl = document.querySelector(".letters");
const letrasErradasEl = document.querySelector(".wrong-letters");
const mensagemEl = document.querySelector(".message");

function iniciarjogo() {
    palavraEscolhida = palavras[Math.floor(Math.random() * palavras.length)];
    letrasCertas = [];
    letrasErradas = [];
    ultimaLetra = "";
    mensagemEl.textContent = "";
    letrasErradasEl.textContent = "";

    palavraEl.textContent = palavraEscolhida
        .split("")
        .map((letra) => (letrasCertas.includes(letra) ? letra : "_"))
        .join(" ");

    letrasEl.innerHTML = "";
    "ABCDEFGHIJKLMNOPQRSTUVWXYZ".split("").forEach((letra) => {
        const button = document.createElement("div");
        button.textContent = letra;
        button.classList.add("letter");
        button.addEventListener("click", () => tentativa(letra));
        letrasEl.appendChild(button);
    });
}

function tentativa(letra) {
    if (letrasCertas.includes(letra) || letrasErradas.includes(letra)) {
        return;
    }

    if (palavraEscolhida.includes(letra)) {
        letrasCertas.push(letra);
    } else {
        letrasErradas.push(letra);
    }

    ultimaLetra = letra;
    atualizarJogo();
}

function atualizarJogo() {
    palavraEl.textContent = palavraEscolhida
        .split("")
        .map((letra) => (letrasCertas.includes(letra) ? letra : "_"))
        .join(" ");

    letrasErradasEl.textContent = `Letras erradas: ${letrasErradas.join(", ")}`;

    if (!palavraEl.textContent.includes("_")) {
        mensagemEl.textContent = "Parabens Venceu 🏆";
        letrasEl.innerHTML = "";
    } else if (letrasErradas.length >= maxTentativas) {
        mensagemEl.textContent = `Você perdeu! A palavra era "${palavraEscolhida} 🏎️".`;
        letrasEl.innerHTML = "";
    }
}


iniciarjogo();