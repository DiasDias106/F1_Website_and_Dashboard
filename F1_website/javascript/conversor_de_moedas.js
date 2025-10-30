const API_KEY = ""; // colocar chave de API aqui

async function converter() {
    const valor = parseFloat(document.getElementById("valor").value);
    const moedaDestino = document.getElementById("moedaDestino").value;
    const url = `https://openexchangerates.org/api/latest.json?app_id=${API_KEY}&symbols=${moedaDestino}`;

    try {
        const resposta = await fetch(url);
        const dados = await resposta.json();
        const taxa = dados.rates[moedaDestino];
        const convertido = (valor * taxa).toFixed(2);
        document.getElementById("resultado").innerText = `${convertido} ${moedaDestino}`;
    } catch (erro) {
        console.error("Erro ao converter moeda:", erro);
        document.getElementById("resultado").innerText = "Erro ao converter.";
    }
}