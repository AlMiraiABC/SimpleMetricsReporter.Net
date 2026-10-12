const charts = new Map();
const metricChartEntries = new Map();
const history = new Map();
const MAX_HISTORY_POINTS = 60;
let pollingTimer = null;
let isPolling = false;
let nextMetricChartId = 0;
const typeColors = { COUNTER: '#5470c6', GAUGE: '#91cc75', HISTOGRAM: '#fac858', SUMMARY: '#ee6666', UNTYPED: '#73c0de' };
const typeLabels = { COUNTER: 'Counter 累计值', GAUGE: 'Gauge 当前值', HISTOGRAM: 'Histogram 分布', SUMMARY: 'Summary 分位数', UNTYPED: 'Untyped' };

const $ = (id) => document.getElementById(id);
const escapeHtml = (value) => String(value).replace(/[&<>"']/g, (char) => ({ '&': '&amp;', '<': '&lt;', '>': '&gt;', '"': '&quot;', "'": '&#039;' }[char]));
const metricType = (metric) => String(metric.type || 'UNTYPED').toUpperCase();
const metricValue = (sample) => Number(sample.value ?? sample.count ?? sample.sum ?? 0);
const labelsText = (sample) => Object.entries(sample.labels || {}).map(([key, value]) => `${key}=${value}`).join(', ');
const seriesKey = (metric, labels) => `${metric.name}{${labelsText({ labels })}}`;

function setStatus(message, isError = false) {
    $('status').textContent = message;
    $('status').className = `small mt-2 ${isError ? 'text-danger' : 'text-secondary'}`;
}

function makeChart(elementId, option) {
    const chart = echarts.init($(elementId));
    chart.setOption(option);
    charts.set(elementId, chart);
    return chart;
}

function getTypeCounts(metrics) {
    return metrics.reduce((result, metric) => {
        const type = metricType(metric);
        result[type] = (result[type] || 0) + 1;
        return result;
    }, {});
}

function updateSummary(metrics) {
    const counts = getTypeCounts(metrics);
    const cards = [
        ['summary-total', '指标数量', metrics.length, '全部已解析的指标'],
        ['summary-counter', 'Counter', counts.COUNTER || 0, '适合折线图/累计趋势'],
        ['summary-gauge', 'Gauge', counts.GAUGE || 0, '适合柱状图/仪表盘'],
        ['summary-distribution', 'Histogram / Summary', (counts.HISTOGRAM || 0) + (counts.SUMMARY || 0), '适合分布图/分位数图']
    ];
    if (!$('summaryCards').children.length) {
        $('summaryCards').innerHTML = cards.map(([id, title, value, hint]) => `
            <div class="col-6 col-xl-3"><div class="card metric-card h-100"><div class="card-body">
                <div class="text-secondary small">${title}</div><div id="${id}" class="metric-value">${value}</div><div class="small text-secondary">${hint}</div>
            </div></div></div>`).join('');
    } else {
        cards.forEach(([id, , value]) => { $(id).textContent = value; });
    }
    $('metricCount').textContent = `${metrics.length} 个指标`;

    const typeData = Object.entries(counts).map(([name, value]) => ({ name: typeLabels[name] || name, value, itemStyle: { color: typeColors[name] } }));
    const overviewData = metrics.map((metric) => ({ value: (metric.metrics || []).length, itemStyle: { color: typeColors[metricType(metric)] || '#999' } }));
    const typeChart = charts.get('typeChart');
    const overviewChart = charts.get('overviewChart');
    if (typeChart) {
        typeChart.setOption({ series: [{ data: typeData }] });
    } else {
        makeChart('typeChart', { tooltip: { trigger: 'item' }, legend: { bottom: 0 }, series: [{ type: 'pie', radius: ['42%', '70%'], data: typeData }] });
    }
    if (overviewChart) {
        overviewChart.setOption({ xAxis: { data: metrics.map((metric) => metric.name) }, series: [{ data: overviewData }] });
    } else {
        makeChart('overviewChart', {
            tooltip: { trigger: 'axis' },
            grid: { left: 45, right: 20, top: 20, bottom: 70 },
            xAxis: { type: 'category', data: metrics.map((metric) => metric.name), axisLabel: { rotate: 35, interval: 0 } },
            yAxis: { type: 'value', name: '样本数' },
            series: [{ type: 'bar', data: overviewData }]
        });
    }
}

function addMetricChart(metric) {
    const type = metricType(metric);
    const id = `metric-chart-${nextMetricChartId++}`;
    const cardId = `metric-card-${id}`;
    $('metricCharts').insertAdjacentHTML('beforeend', `
        <div id="${cardId}" class="col-12 col-lg-6"><div class="card chart-card h-100">
            <div class="card-body"><div class="d-flex justify-content-between gap-2">
                <div><h3 class="h6 metric-name mb-1">${escapeHtml(metric.name)}</h3><div class="small text-secondary">${escapeHtml(metric.help || '无描述')}</div></div>
                <span class="badge align-self-start" style="background:${typeColors[type] || '#6c757d'}">${type}</span>
            </div><div id="${id}" class="chart"></div></div>
        </div></div>`);
    metricChartEntries.set(metric.name, { id, cardId, type, initialized: false });
    makeChart(id, {});
}

function updateMetricChart(metric) {
    const entry = metricChartEntries.get(metric.name);
    const chart = charts.get(entry.id);
    const series = getMetricSeries(metric).map((item) => ({
        id: item.key,
        name: item.name,
        type: 'line',
        showSymbol: false,
        smooth: true,
        data: history.get(item.key) || []
    }));
    if (entry.initialized) {
        chart.setOption({ series }, { replaceMerge: ['series'] });
    } else {
        chart.setOption({
            tooltip: { trigger: 'axis' },
            legend: { type: 'scroll', bottom: 0 },
            grid: { left: 65, right: 25, top: 25, bottom: 90 },
            xAxis: {
                type: 'time',
                splitNumber: 5,
                minInterval: 5000,
                axisLabel: {
                    hideOverlap: true,
                    formatter: (value) => new Date(value).toLocaleTimeString('zh-CN', {
                        hour: '2-digit',
                        minute: '2-digit',
                        second: '2-digit'
                    })
                }
            },
            yAxis: { type: 'value', name: '数值', scale: true },
            dataZoom: [
                { type: 'inside', xAxisIndex: 0, filterMode: 'none' },
                { type: 'slider', xAxisIndex: 0, filterMode: 'none', start: 70, end: 100, height: 18, bottom: 28, showDetail: false }
            ],
            series
        });
        entry.initialized = true;
    }
}

function render(metrics) {
    updateSummary(metrics);
    const incomingNames = new Set(metrics.map((metric) => metric.name));
    metricChartEntries.forEach((entry, name) => {
        if (!incomingNames.has(name) || entry.type !== metricType(metrics.find((metric) => metric.name === name))) {
            charts.get(entry.id).dispose();
            charts.delete(entry.id);
            $(entry.cardId).remove();
            metricChartEntries.delete(name);
        }
    });
    metrics.forEach((metric) => {
        if (!metricChartEntries.has(metric.name)) addMetricChart(metric);
        updateMetricChart(metric);
    });
    $('jsonOutput').textContent = JSON.stringify(metrics, null, 2);
}

function getMetricSeries(metric) {
    const type = metricType(metric);
    const result = [];
    (metric.metrics || []).forEach((sample, sampleIndex) => {
        const labels = sample.labels || {};
        if (type === 'HISTOGRAM') {
            Object.entries(sample.buckets || {}).forEach(([bucket, value]) => {
                const bucketLabels = { ...labels, le: bucket };
                const labelPrefix = labelsText(sample);
                result.push({ key: seriesKey(metric, bucketLabels), name: `${labelPrefix ? `${labelPrefix}, ` : ''}le=${bucket}`, value: Number(value) });
            });
        } else if (type === 'SUMMARY' && sample.quantiles) {
            Object.entries(sample.quantiles).forEach(([quantile, value]) => {
                const quantileLabels = { ...labels, quantile };
                const labelPrefix = labelsText(sample);
                result.push({ key: seriesKey(metric, quantileLabels), name: `${labelPrefix ? `${labelPrefix}, ` : ''}quantile=${quantile}`, value: Number(value) });
            });
        } else {
            result.push({ key: seriesKey(metric, labels), name: labelsText(sample) || `样本 ${sampleIndex + 1}`, value: metricValue(sample) });
        }
    });
    return result;
}

function appendHistory(metrics, timestamp) {
    metrics.forEach((metric) => {
        getMetricSeries(metric).forEach((item) => {
            const points = history.get(item.key) || [];
            points.push([timestamp, item.value]);
            if (points.length > MAX_HISTORY_POINTS) points.splice(0, points.length - MAX_HISTORY_POINTS);
            history.set(item.key, points);
        });
    });
}

async function fetchMetrics() {
    if (isPolling) return;
    isPolling = true;
    try {
        const endpoint = 'prometheus';
        const response = await fetch(endpoint, { cache: 'no-store' });
        if (!response.ok) throw new Error(`HTTP ${response.status}`);
        const text = await response.text();
        const parsed = window.parsePrometheusTextFormat(text);
        if (!Array.isArray(parsed)) throw new Error('解析器返回的数据不是数组');
        $('sourceText').textContent = text;
        appendHistory(parsed, Date.now());
        render(parsed);
        setStatus(`已从 ${endpoint} 获取 ${parsed.length} 个指标；最后刷新：${new Date().toLocaleTimeString()}`);
    } catch (error) {
        setStatus(`接口刷新失败：${error.message}。请确认服务已启动且接口允许跨域请求。`, true);
    } finally {
        isPolling = false;
    }
}

function startPolling() {
    if (pollingTimer) return;
    history.clear();
    fetchMetrics();
    pollingTimer = window.setInterval(fetchMetrics, 1000);
    $('startPolling').disabled = true;
    $('stopPolling').disabled = false;
}

function stopPolling() {
    if (pollingTimer) window.clearInterval(pollingTimer);
    pollingTimer = null;
    $('startPolling').disabled = false;
    $('stopPolling').disabled = true;
    setStatus('已停止实时刷新。');
}

$('startPolling').addEventListener('click', startPolling);
$('stopPolling').addEventListener('click', stopPolling);
window.addEventListener('resize', () => charts.forEach((chart) => chart.resize()));
window.addEventListener('load', startPolling);
