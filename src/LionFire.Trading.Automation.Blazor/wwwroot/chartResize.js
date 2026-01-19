// Chart resize observer for Plotly charts
window.chartResizeObserver = {
    observers: new Map(),
    mutationObserver: null,
    resizeTimeout: null,

    observe: function (elementId, dotNetHelper) {
        const element = document.getElementById(elementId);
        if (!element) {
            console.warn('chartResizeObserver: element not found:', elementId);
            return;
        }

        const resizeChart = () => {
            const plotlyElements = element.querySelectorAll('.js-plotly-plot');
            if (plotlyElements.length > 0 && window.Plotly) {
                plotlyElements.forEach(plotlyEl => {
                    try {
                        // Use relayout with autosize to force a full redraw
                        window.Plotly.relayout(plotlyEl, { autosize: true });
                    } catch (e) {
                        // Ignore errors
                    }
                });
            }
        };

        const resizeObserver = new ResizeObserver(entries => {
            requestAnimationFrame(resizeChart);
        });

        resizeObserver.observe(element);
        this.observers.set(elementId, resizeObserver);
    },

    unobserve: function (elementId) {
        const observer = this.observers.get(elementId);
        if (observer) {
            observer.disconnect();
            this.observers.delete(elementId);
        }
    },

    resizeAllCharts: function () {
        if (!window.Plotly) return;

        const plotlyElements = document.querySelectorAll('.js-plotly-plot');
        plotlyElements.forEach(plotlyEl => {
            try {
                // Use relayout with autosize to force a full redraw
                window.Plotly.relayout(plotlyEl, { autosize: true });
            } catch (e) {
                // Ignore errors
            }
        });
    },

    // Initialize global GridStack observer
    initGridStackObserver: function () {
        if (this.mutationObserver) return;

        const self = this;

        // Debounced resize function
        const debouncedResize = () => {
            clearTimeout(self.resizeTimeout);
            self.resizeTimeout = setTimeout(() => {
                self.resizeAllCharts();
            }, 50);
        };

        // Create a MutationObserver that watches for style changes on grid items
        this.mutationObserver = new MutationObserver((mutations) => {
            for (const mutation of mutations) {
                if (mutation.type === 'attributes') {
                    const attrName = mutation.attributeName;
                    // GridStack updates these attributes during resize/drag
                    if (attrName === 'style' || attrName === 'gs-x' || attrName === 'gs-y' ||
                        attrName === 'gs-w' || attrName === 'gs-h' || attrName === 'class') {
                        debouncedResize();
                        break;
                    }
                }
            }
        });

        // Observe the entire document for grid-stack-item changes
        const observeGridItems = () => {
            const gridItems = document.querySelectorAll('.grid-stack-item');
            gridItems.forEach(item => {
                this.mutationObserver.observe(item, {
                    attributes: true,
                    attributeFilter: ['style', 'class', 'gs-x', 'gs-y', 'gs-w', 'gs-h']
                });
            });

            // Also observe the grid-stack container for new items
            const gridContainers = document.querySelectorAll('.grid-stack');
            gridContainers.forEach(container => {
                this.mutationObserver.observe(container, {
                    childList: true,
                    subtree: false
                });
            });
        };

        // Initial observation
        observeGridItems();

        // Re-observe when new items are added (check periodically)
        setInterval(observeGridItems, 2000);

        // Also listen for window resize
        window.addEventListener('resize', debouncedResize);

        console.log('chartResizeObserver: GridStack observer initialized');
    }
};

// Initialize the GridStack observer when DOM is ready
if (document.readyState === 'loading') {
    document.addEventListener('DOMContentLoaded', () => {
        setTimeout(() => window.chartResizeObserver.initGridStackObserver(), 500);
    });
} else {
    setTimeout(() => window.chartResizeObserver.initGridStackObserver(), 500);
}
