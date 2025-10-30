function converterImagem(formato) {
    const input = document.getElementById('inputImagem');
    const img = document.getElementById('imagemOriginal');
    const output = document.getElementById('imagemConvertida');

    if (input.files && input.files[0]) {
        const reader = new FileReader();
        reader.onload = function (e) {
            img.src = e.target.result;

            const canvas = document.createElement('canvas');
            const ctx = canvas.getContext('2d');
            const image = new Image();

            image.onload = function () {
                canvas.width = image.width;
                canvas.height = image.height;
                ctx.drawImage(image, 0, 0);

                const dataURL = canvas.toDataURL(formato);
                output.src = dataURL;

                const link = document.createElement('a');
                link.href = dataURL;
                link.download = `imagem_convertida.${formato.split('/')[1]}`;
                document.body.appendChild(link);
                link.click();
                document.body.removeChild(link);
            };

            image.src = e.target.result;
        };
        reader.readAsDataURL(input.files[0]);
    }
}
