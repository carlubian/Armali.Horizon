// ── Expense Charts (Chart.js) ────────────────────────────────────
// Cada canvas se identifica por su id; las instancias se almacenan en un mapa
// para destruirlas antes de re-renderizar.

const _expChartInstances = {};

function _destroyIfExists(id) {
    if (_expChartInstances[id]) {
        _expChartInstances[id].destroy();
        delete _expChartInstances[id];
    }
}

// Paleta de colores generada automáticamente para gráficos circulares
const _piePalette = [
    '#22c55e', '#3b82f6', '#ef4444', '#f59e0b', '#8b5cf6',
    '#06b6d4', '#ec4899', '#14b8a6', '#f97316', '#6366f1',
    '#84cc16', '#e11d48', '#0ea5e9', '#a855f7', '#10b981',
    '#d946ef', '#facc15', '#64748b', '#fb923c', '#2dd4bf'
];

// Opciones compartidas para tema oscuro
const _darkScaleOpts = {
    y: {
        beginAtZero: true,
        ticks: { color: '#9ca3af' },
        grid: { color: 'rgba(75, 85, 99, 0.3)' }
    },
    x: {
        ticks: { color: '#9ca3af' },
        grid: { color: 'rgba(75, 85, 99, 0.3)' }
    }
};

const _darkLegend = {
    display: true,
    position: 'top',
    labels: { color: '#9ca3af', usePointStyle: true, padding: 15 }
};

// ── Gráfico de línea: Income (verde) + Expense (rojo) ────────────

function renderExpenseLineChart(canvasId, labels, incomeData, expenseData) {
    const canvas = document.getElementById(canvasId);
    if (!canvas) return;
    _destroyIfExists(canvasId);

    _expChartInstances[canvasId] = new Chart(canvas.getContext('2d'), {
        type: 'line',
        data: {
            labels: labels,
            datasets: [
                {
                    label: 'Income',
                    data: incomeData,
                    borderColor: '#22c55e',
                    backgroundColor: 'rgba(34, 197, 94, 0.1)',
                    borderWidth: 2,
                    tension: 0.3,
                    pointRadius: 4,
                    pointBackgroundColor: '#22c55e',
                    fill: true,
                    spanGaps: true
                },
                {
                    label: 'Expense',
                    data: expenseData,
                    borderColor: '#ef4444',
                    backgroundColor: 'rgba(239, 68, 68, 0.1)',
                    borderWidth: 2,
                    tension: 0.3,
                    pointRadius: 4,
                    pointBackgroundColor: '#ef4444',
                    fill: true,
                    spanGaps: true
                }
            ]
        },
        options: {
            responsive: true,
            maintainAspectRatio: false,
            scales: _darkScaleOpts,
            plugins: { legend: _darkLegend }
        }
    });
}

// ── Gráfico de línea: Year-over-Year (año anterior gris punteado + año actual azul) ─

function renderExpenseYoyChart(canvasId, labels, prevExpense, currExpense, prevYear, currYear) {
    const canvas = document.getElementById(canvasId);
    if (!canvas) return;
    _destroyIfExists(canvasId);

    _expChartInstances[canvasId] = new Chart(canvas.getContext('2d'), {
        type: 'line',
        data: {
            labels: labels,
            datasets: [
                {
                    label: prevYear.toString(),
                    data: prevExpense,
                    borderColor: '#9ca3af',
                    backgroundColor: 'rgba(156, 163, 175, 0.1)',
                    borderWidth: 2,
                    borderDash: [6, 3],
                    tension: 0.3,
                    pointRadius: 3,
                    pointBackgroundColor: '#9ca3af',
                    fill: true,
                    spanGaps: true
                },
                {
                    label: currYear.toString(),
                    data: currExpense,
                    borderColor: '#3b82f6',
                    backgroundColor: 'rgba(59, 130, 246, 0.1)',
                    borderWidth: 2,
                    tension: 0.3,
                    pointRadius: 4,
                    pointBackgroundColor: '#3b82f6',
                    fill: true,
                    spanGaps: true
                }
            ]
        },
        options: {
            responsive: true,
            maintainAspectRatio: false,
            scales: _darkScaleOpts,
            plugins: { legend: _darkLegend }
        }
    });
}

// ── Gráfico circular (doughnut) ──────────────────────────────────

function renderExpensePieChart(canvasId, labels, data) {
    const canvas = document.getElementById(canvasId);
    if (!canvas) return;
    _destroyIfExists(canvasId);

    // Si no hay datos, destruir el chart anterior y dejar el canvas vacío
    if (!data || data.length === 0 || data.every(v => v === 0)) {
        canvas.getContext('2d').clearRect(0, 0, canvas.width, canvas.height);
        return;
    }

    const colors = labels.map((_, i) => _piePalette[i % _piePalette.length]);

    _expChartInstances[canvasId] = new Chart(canvas.getContext('2d'), {
        type: 'doughnut',
        data: {
            labels: labels,
            datasets: [{
                data: data,
                backgroundColor: colors,
                borderColor: '#1e1e2e',
                borderWidth: 2
            }]
        },
        options: {
            responsive: true,
            maintainAspectRatio: false,
            plugins: {
                legend: {
                    display: true,
                    position: 'right',
                    labels: { color: '#9ca3af', padding: 10, usePointStyle: true }
                }
            }
        }
    });
}

// ── Destruir todos los gráficos de Expenses ──────────────────────

function destroyAllExpenseCharts() {
    for (const id of Object.keys(_expChartInstances)) {
        _expChartInstances[id].destroy();
        delete _expChartInstances[id];
    }
}

