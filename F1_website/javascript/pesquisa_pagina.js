document.getElementById("pesquisa").addEventListener("keyup", function () {
    const termo = this.value.toLowerCase();
    const ssDiv = document.getElementById("ss");

    if (termo.length > 0) {
        fetch('/Pesquisas/GetPaginas')
            .then(response => response.json())
            .then(data => {
                ssDiv.innerHTML = "";
                ssDiv.classList.add('show');

                const resultados = data.filter(item => item.titulo.toLowerCase().includes(termo));

                if (resultados.length === 0) {
                    const noResult = document.createElement("div");
                    noResult.textContent = "Nenhuma sugestão encontrada";
                    noResult.className = "sug";
                    ssDiv.appendChild(noResult);
                } else {
                    resultados.forEach(item => {
                        const div = document.createElement("div");
                        div.className = "sug";
                        div.textContent = item.titulo;
                        div.addEventListener("click", function () {
                            window.location.href = item.caminho.replace("~", "");
                        });
                        ssDiv.appendChild(div);
                    });
                }
            })
            .catch(error => console.error("Erro ao buscar sugestões:", error));
    } else {
        ssDiv.classList.remove('show');
        ssDiv.innerHTML = "";
    }
});
