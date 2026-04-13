// Instancias de gráficos para la página Mood Trends
let moodTrendsWeeklyInstance = null;
let moodTrendsWeekdayInstance = null;
let moodTrendsEvoMonthInstance = null;
let moodTrendsEvoWeekdayInstance = null;

// Configuración compartida para gráficos min/avg/max
function _buildMinAvgMaxConfig(labels, minData, avgData, maxData) {
    return {
        type: 'line',
        data: {
            labels: labels,
            datasets: [
                {
                    label: 'Min',
                    data: minData,
                    borderColor: '#ef4444',
                    backgroundColor: 'rgba(239, 68, 68, 0.1)',
                    borderWidth: 2,
                    tension: 0.3,
                    pointRadius: 3,
                    pointBackgroundColor: '#ef4444',
                    spanGaps: true
                },
                {
                    label: 'Avg',
                    data: avgData,
                    borderColor: '#3b82f6',
                    backgroundColor: 'rgba(59, 130, 246, 0.1)',
                    borderWidth: 2,
                    tension: 0.3,
                    pointRadius: 3,
                    pointBackgroundColor: '#3b82f6',
                    spanGaps: true
                },
                {
                    label: 'Max',
                    data: maxData,
                    borderColor: '#22c55e',
                    backgroundColor: 'rgba(34, 197, 94, 0.1)',
                    borderWidth: 2,
                    tension: 0.3,
                    pointRadius: 3,
                    pointBackgroundColor: '#22c55e',
                    spanGaps: true
                }
            ]
        },
        options: {
            responsive: true,
            maintainAspectRatio: false,
            scales: {
                y: {
                    min: 0,
                    max: 5,
                    ticks: { stepSize: 1, color: '#9ca3af' },
                    grid: { color: 'rgba(75, 85, 99, 0.3)' }
                },
                x: {
                    ticks: { color: '#9ca3af' },
                    grid: { color: 'rgba(75, 85, 99, 0.3)' }
                }
            },
            plugins: {
                legend: {
                    display: true,
                    position: 'top',
                    labels: { color: '#9ca3af', usePointStyle: true, padding: 15 }
                }
            }
        }
    };
}

// Configuración compartida para gráficos de comparación interanual (dos series: año anterior y año actual)
function _buildYoyConfig(labels, prevData, currData, prevYear, currYear) {
    return {
        type: 'line',
        data: {
            labels: labels,
            datasets: [
                {
                    label: prevYear.toString(),
                    data: prevData,
                    borderColor: '#9ca3af',
                    backgroundColor: 'rgba(156, 163, 175, 0.1)',
                    borderWidth: 2,
                    borderDash: [6, 3],
                    tension: 0.3,
                    pointRadius: 3,
                    pointBackgroundColor: '#9ca3af',
                    spanGaps: true
                },
                {
                    label: currYear.toString(),
                    data: currData,
                    borderColor: '#3b82f6',
                    backgroundColor: 'rgba(59, 130, 246, 0.1)',
                    borderWidth: 2,
                    tension: 0.3,
                    pointRadius: 4,
                    pointBackgroundColor: '#3b82f6',
                    spanGaps: true
                }
            ]
        },
        options: {
            responsive: true,
            maintainAspectRatio: false,
            scales: {
                y: {
                    min: 0,
                    max: 5,
                    ticks: { stepSize: 1, color: '#9ca3af' },
                    grid: { color: 'rgba(75, 85, 99, 0.3)' }
                },
                x: {
                    ticks: { color: '#9ca3af' },
                    grid: { color: 'rgba(75, 85, 99, 0.3)' }
                }
            },
            plugins: {
                legend: {
                    display: true,
                    position: 'top',
                    labels: { color: '#9ca3af', usePointStyle: true, padding: 15 }
                }
            }
        }
    };
}

// Gráfico semanal (min/avg/max por semana ISO del año)
function renderMoodTrendsWeekly(labels, minData, avgData, maxData) {
    const canvas = document.getElementById('moodTrendsWeekly');
    if (!canvas) return;
    if (moodTrendsWeeklyInstance) moodTrendsWeeklyInstance.destroy();
    moodTrendsWeeklyInstance = new Chart(canvas.getContext('2d'),
        _buildMinAvgMaxConfig(labels, minData, avgData, maxData));
}

// Gráfico por día de la semana (min/avg/max agregado lunes a domingo)
function renderMoodTrendsWeekday(labels, minData, avgData, maxData) {
    const canvas = document.getElementById('moodTrendsWeekday');
    if (!canvas) return;
    if (moodTrendsWeekdayInstance) moodTrendsWeekdayInstance.destroy();
    moodTrendsWeekdayInstance = new Chart(canvas.getContext('2d'),
        _buildMinAvgMaxConfig(labels, minData, avgData, maxData));
}

// Comparación interanual de medias por mes
function renderMoodTrendsEvoMonth(labels, prevData, currData, prevYear, currYear) {
    const canvas = document.getElementById('moodTrendsEvoMonth');
    if (!canvas) return;
    if (moodTrendsEvoMonthInstance) moodTrendsEvoMonthInstance.destroy();
    moodTrendsEvoMonthInstance = new Chart(canvas.getContext('2d'),
        _buildYoyConfig(labels, prevData, currData, prevYear, currYear));
}

// Comparación interanual de medias por día de la semana
function renderMoodTrendsEvoWeekday(labels, prevData, currData, prevYear, currYear) {
    const canvas = document.getElementById('moodTrendsEvoWeekday');
    if (!canvas) return;
    if (moodTrendsEvoWeekdayInstance) moodTrendsEvoWeekdayInstance.destroy();
    moodTrendsEvoWeekdayInstance = new Chart(canvas.getContext('2d'),
        _buildYoyConfig(labels, prevData, currData, prevYear, currYear));
}

