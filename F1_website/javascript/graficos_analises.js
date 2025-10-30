document.addEventListener('DOMContentLoaded', function () {
    const speedCtx = document.getElementById('speedChart').getContext('2d');
    const speedChart = new Chart(speedCtx, {
        type: 'line',
        data: {
            labels: ['Volta 1', 'Volta 2', 'Volta 3', 'Volta 4'],
            datasets: [{
                label: 'Velocidade (km/h)',
                data: [280, 290, 285, 295],
                borderColor: '#d90429',
                backgroundColor: 'rgba(217, 4, 41, 0.2)',
                fill: true
            }]
        }
    });

    const conditionCtx = document.getElementById('trackConditionsChart').getContext('2d');
    const conditionChart = new Chart(conditionCtx, {
        type: 'bar',
        data: {
            labels: ['Temperatura (°C)', 'Humidade (%)', 'Pista'],
            datasets: [{
                label: 'Condições',
                data: [32, 55, 1],
                backgroundColor: ['orange', 'blue', 'green']
            }]
        },
        options: {
            scales: {
                y: { beginAtZero: true }
            }
        }
    });
});
