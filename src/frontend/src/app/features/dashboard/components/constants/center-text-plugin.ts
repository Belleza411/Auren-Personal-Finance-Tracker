export const centerTextPlugin = {
    id: 'centerText',
    afterDraw(chart: any) {
        if(chart.config.type !== 'doughnut') return;

        const { ctx, chartArea } = chart;
        if(!chartArea) return;

        const { left, right, top, bottom } = chartArea;
        const centerX = (left + right) / 2;
        const centerY = (top + bottom) / 2;

        ctx.save();
        ctx.textAlign = 'center';
        ctx.textBaseline = 'middle';

        ctx.font = 'bold 32px Arial';
        ctx.fillText('$' + chart.config.data.datasets[0].data.reduce((a: number, b: number) => a + b, 0), centerX, centerY - 10);


        ctx.font = '14px Arial';
        ctx.fillStyle = '#666';
        ctx.fillText('Total Spent', centerX, centerY + 20);

        ctx.restore();
    }
}