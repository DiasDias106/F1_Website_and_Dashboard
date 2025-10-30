const display = document.getElementById("display");

function adicionaraodisplay(input) {
    display.value += input;
}
function limpardisplay() {
    display.value = "";
}
function calcular() {
    try {
        display.value = eval(display.value);
    } catch {
        display.value = "Error";
    }
}