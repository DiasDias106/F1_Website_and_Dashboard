let count = 0;
let numeroescolhido = Math.floor(Math.random() * 50) + 1;

adnumjs.onclick = function jogar() {
    let nome = document.getElementById("nome1").value;
    let numero_introduzido = parseInt(document.getElementById("txtadnum").value);
    let resultado = document.getElementById("visualizador");

    if (!nome || isNaN(numero_introduzido)) {
        resultado.innerHTML = "Por favor, preencha todos os campos corretamente!";
        return;
    }

    if (count >= 5) {
        resultado.innerHTML = `As tentativas acabaram! O número era ${numeroescolhido}.`;
        return;
    }

    if (numero_introduzido === numeroescolhido) {
        resultado.innerHTML = "Parabéns! Acertou o número!";
    } else if (numero_introduzido > numeroescolhido) {
        resultado.innerHTML = "Tente novamente! O número é menor.";
    } else {
        resultado.innerHTML = "Tente novamente! O número é maior.";
    }

    count++;

    if (count === 5 && numero_introduzido !== numeroescolhido) {
        resultado.innerHTML = `As tentativas acabaram! O número era ${numeroescolhido}.`;
    }
};