//function lerTexto() {
//    const texto = document.getElementById("texto").value;
//    if (texto) {
//        const msg = new SpeechSynthesisUtterance(texto);
//        window.speechSynthesis.speak(msg);
//    } else {
//        alert("Por favor, digite uma palavra.");
//    }
//}


let animacaoID = null;

function lerTexto() {
    const texto = document.getElementById("texto").value;

    if (texto) {
        const msg = new SpeechSynthesisUtterance(texto);

        msg.onstart = () => {
            iniciarVisualizador();
        };

        msg.onend = () => {
            pararVisualizador();
        };

        window.speechSynthesis.speak(msg);
    } else {
        alert("Por favor, introduza uma palavra.");
    }
}

function iniciarVisualizador() {
    const canvas = document.getElementById("visualizador");
    const ctx = canvas.getContext("2d");

    canvas.width = window.innerWidth;
    canvas.height = 150;

    const barras = 64;

    function desenhar() {
        ctx.clearRect(0, 0, canvas.width, canvas.height);

        const larguraBarra = canvas.width / barras;

        for (let i = 0; i < barras; i++) {
            const altura = Math.random() * canvas.height;
            const x = i * larguraBarra;
            ctx.fillStyle = `rgb(${100 + Math.random() * 155}, 0,0)`;
            ctx.fillRect(x, canvas.height - altura, larguraBarra - 2, altura);
        }

        animacaoID = requestAnimationFrame(desenhar);
    }

    desenhar();
}

function pararVisualizador() {
    cancelAnimationFrame(animacaoID);
    const canvas = document.getElementById("visualizador");
    const ctx = canvas.getContext("2d");
    ctx.clearRect(0, 0, canvas.width, canvas.height);
}

