function encrypt() {
    const method = document.getElementById("methodSelect").value;
    const text = document.getElementById("textInput").value;
    const output = document.getElementById("output");

    if (!text) {
        output.textContent = "⚠️ Introduza um texto um texto.";
        return;
    }

    let result = "";

    if (method === "aes") {
        result = CryptoJS.AES.encrypt(text, "minha-chave-secreta").toString();
    } else if (method === "sha256") {
        result = CryptoJS.SHA256(text).toString();
    } else {
        result = "❌ Método de encriptação não suportado.";
    }

    output.textContent = `Texto Encriptado: \n${result}`;
}

function decrypt() {
    const method = document.getElementById("methodSelect").value;
    const text = document.getElementById("textInput").value;
    const output = document.getElementById("output");

    if (!text) {
        output.textContent = "⚠️ Digite um texto.";
        return;
    }

    let result = "";

    if (method === "aes") {
        try {
            const bytes = CryptoJS.AES.decrypt(text, "minha-chave-secreta");
            result = bytes.toString(CryptoJS.enc.Utf8);
            if (!result) {
                result = "❌ A desencriptação falhou.";
            }
        } catch (e) {
            result = "❌ Erro na desencriptação.";
        }
    } else {
        result = "❌ Desencriptação não é suportada para este método.";
    }

    output.textContent = `Texto Desencriptado: \n${result}`;
}