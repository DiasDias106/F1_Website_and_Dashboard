function iniciarFundoReativo() {
    if (intervaloSom) return;

    intervaloSom = setInterval(() => {
        const intensidade = Math.floor(Math.random() * 100);
        const verde = Math.min(255, intensidade * 2.5);
        document.getElementById("reactive-bg").style.backgroundColor = `rgb(0, ${verde}, 255)`;
    }, 100);
}

function pararFundoReativo() {
    clearInterval(intervaloSom);
    intervaloSom = null;
    document.getElementById("reactive-bg").style.backgroundColor = "#001f3f";
}
