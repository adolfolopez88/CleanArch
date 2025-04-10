// Custom Swagger UI script to display cURL examples
(function () {
    // Execute after Swagger UI is fully loaded
    window.addEventListener('load', function() {
        // Give some time for Swagger UI to initialize
        setTimeout(function() {
            // Hook to render the cURL examples
            const observer = new MutationObserver(function(mutations) {
                mutations.forEach(function(mutation) {
                    if (mutation.type === 'childList' && mutation.addedNodes.length > 0) {
                        renderCurlExamples();
                    }
                });
            });
            
            // Start observing the opblock-body element for changes
            observer.observe(document.getElementById('swagger-ui'), {
                childList: true,
                subtree: true
            });
            
            // Initial render
            renderCurlExamples();
        }, 1000);
    });
    
    function renderCurlExamples() {
        // Find all expanded operations
        const operations = document.querySelectorAll('.opblock.is-open');
        
        operations.forEach(function(operation) {
            // Check if we already added the cURL example
            const hasExample = operation.querySelector('.curl-example');
            if (hasExample) {
                return;
            }
            
            // Find the operation ID and path
            const opblock = operation.querySelector('.opblock-summary');
            if (!opblock) return;
            
            // Get the method and path from the DOM
            const method = operation.className.match(/opblock-(get|put|post|delete|patch)/)?.[1]?.toUpperCase();
            if (!method) return;
            
            // Get the path from the operation
            const pathElement = opblock.querySelector('.opblock-summary-path');
            if (!pathElement) return;
            const path = pathElement.getAttribute('data-path');
            if (!path) return;
            
            // Find operation in Swagger UI spec
            const spec = window.ui.getState().get('spec').toJS();
            if (!spec || !spec.paths) return;
            
            // Find the operation in the spec
            let curlExample = null;
            
            if (spec.paths[path] && spec.paths[path][method.toLowerCase()]) {
                const op = spec.paths[path][method.toLowerCase()];
                if (op && op['x-curl-example']) {
                    curlExample = op['x-curl-example'];
                }
            }
            
            if (!curlExample) return;
            
            // Create cURL example section
            const curlSection = document.createElement('div');
            curlSection.className = 'curl-example opblock-section';
            curlSection.innerHTML = `
                <div class="opblock-section-header">
                    <h4>cURL Example</h4>
                </div>
                <div class="opblock-description-wrapper">
                    <pre class="curl-command" style="background-color: #333; color: #fff; padding: 10px; border-radius: 4px; overflow-x: auto;">${curlExample}</pre>
                    <button class="btn copy-curl" style="margin-top: 5px;">Copy to Clipboard</button>
                </div>
            `;
            
            // Add the example after the request or response body
            const footer = operation.querySelector('.opblock-section:last-child');
            if (footer) {
                footer.parentNode.insertBefore(curlSection, footer.nextSibling);
                
                // Add copy to clipboard functionality
                const copyBtn = curlSection.querySelector('.copy-curl');
                copyBtn.addEventListener('click', function() {
                    const el = document.createElement('textarea');
                    el.value = curlExample;
                    document.body.appendChild(el);
                    el.select();
                    document.execCommand('copy');
                    document.body.removeChild(el);
                    
                    // Show copied message
                    copyBtn.textContent = 'Copied!';
                    setTimeout(function() {
                        copyBtn.textContent = 'Copy to Clipboard';
                    }, 2000);
                });
            }
        });
    }
})();