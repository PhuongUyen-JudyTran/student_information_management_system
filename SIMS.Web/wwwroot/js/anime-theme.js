(function () {
    // 1. Dynamically load the anime theme stylesheet
    const cssPath = '/css/anime-theme.css';
    if (!document.querySelector(`link[href="${cssPath}"]`)) {
        const link = document.createElement('link');
        link.rel = 'stylesheet';
        link.href = cssPath;
        document.head.appendChild(link);
    }

    // 2. Sakura blossom generator (Sakura Theme - gentle vertical falling)
    function initSakuraPetals() {
        let container = document.querySelector('.sakura-container');
        if (!container) {
            container = document.createElement('div');
            container.className = 'sakura-container';
            document.body.appendChild(container);
        }
        container.innerHTML = '';

        const maxPetals = 22;
        for (let i = 0; i < maxPetals; i++) {
            const petal = document.createElement('div');
            petal.className = 'sakura-petal';
            
            const size = Math.random() * 9 + 6;
            petal.style.width = `${size}px`;
            petal.style.height = `${size}px`;
            petal.style.left = `${Math.random() * 100}%`;
            petal.style.animationDuration = `${Math.random() * 8 + 6}s`;
            petal.style.animationDelay = `${Math.random() * -10}s`;
            petal.style.opacity = (Math.random() * 0.4 + 0.5).toFixed(2);

            container.appendChild(petal);
        }
    }

    // 3. Sakura Tornado / Vortex generator (Storm Theme)
    function initSakuraStorm() {
        let container = document.querySelector('.storm-container');
        if (!container) {
            container = document.createElement('div');
            container.className = 'storm-container';
            document.body.appendChild(container);
        }
        container.innerHTML = '';

        const maxPetals = 35;
        for (let i = 0; i < maxPetals; i++) {
            const petal = document.createElement('div');
            petal.className = 'tornado-petal';
            
            const size = Math.random() * 8 + 6;
            petal.style.width = `${size}px`;
            petal.style.height = `${size}px`;

            // Dynamic orbital offsets to create a spiral tornado vortex path
            const startX = Math.random() * 200 - 100; // -100px to 100px from center
            const midX = (startX > 0 ? -1 : 1) * (Math.random() * 200 + 100); // swing out to opposite side
            const endX = Math.random() * 300 - 150; // final disperses at top

            petal.style.setProperty('--orbit-x-start', `${startX}px`);
            petal.style.setProperty('--orbit-x-mid', `${midX}px`);
            petal.style.setProperty('--orbit-x-end', `${endX}px`);

            // Tornado speed is faster (3.5s to 7s) with random delay
            petal.style.animationDuration = `${Math.random() * 3.5 + 3.5}s`;
            petal.style.animationDelay = `${Math.random() * -6}s`;
            petal.style.opacity = (Math.random() * 0.4 + 0.6).toFixed(2);

            container.appendChild(petal);
        }
    }

    // 4. Wuthering Waves (WuWa) Floating Resonance Particles generator
    function initWuWaWaves() {
        let container = document.querySelector('.wuwa-container');
        if (!container) {
            container = document.createElement('div');
            container.className = 'wuwa-container';
            document.body.appendChild(container);
        }
        container.innerHTML = '';

        const maxParticles = 25;
        for (let i = 0; i < maxParticles; i++) {
            const particle = document.createElement('div');
            particle.className = 'wuwa-particle';
            
            const size = Math.random() * 8 + 4;
            particle.style.width = `${size}px`;
            particle.style.height = `${size}px`;
            particle.style.left = `${Math.random() * 100}%`;
            
            const driftX = Math.random() * 120 - 60;
            particle.style.setProperty('--wuwa-drift-x', `${driftX}px`);

            particle.style.animationDuration = `${Math.random() * 6 + 5}s`;
            particle.style.animationDelay = `${Math.random() * -8}s`;
            particle.style.opacity = (Math.random() * 0.4 + 0.4).toFixed(2);

            container.appendChild(particle);
        }
    }

    // 5. Gargantua Black Hole Gravitational Pull generator
    function initGargantua() {
        // Accretion disk injection
        let singularityContainer = document.querySelector('.gargantua-singularity-container');
        if (!singularityContainer) {
            singularityContainer = document.createElement('div');
            singularityContainer.className = 'gargantua-singularity-container';
            singularityContainer.innerHTML = `
                <div class="gargantua-singularity"></div>
                <div class="gargantua-disk"></div>
            `;
            document.body.appendChild(singularityContainer);
        }

        // Photons container injection
        let photonsContainer = document.querySelector('.gargantua-photons');
        if (!photonsContainer) {
            photonsContainer = document.createElement('div');
            photonsContainer.className = 'gargantua-photons';
            document.body.appendChild(photonsContainer);
        }
        photonsContainer.innerHTML = '';

        const maxPhotons = 35;
        const centerX = window.innerWidth / 2;
        const centerY = window.innerHeight / 2;

        for (let i = 0; i < maxPhotons; i++) {
            const photon = document.createElement('div');
            photon.className = 'photon-particle';
            
            const size = Math.random() * 5 + 3;
            photon.style.width = `${size}px`;
            photon.style.height = `${size}px`;
            photon.style.left = '0';
            photon.style.top = '0';

            const angle = Math.random() * Math.PI * 2;
            const startRadius = Math.max(window.innerWidth, window.innerHeight) * 0.7;
            
            const startX = centerX + Math.cos(angle) * startRadius;
            const startY = centerY + Math.sin(angle) * startRadius;

            // Spiral pull: spiral slightly in orbit around singularity (add rotation angle offset)
            const spiralOffset = Math.random() * 2 + 1; // 1 to 3 radians twist
            const endRadius = Math.random() * 40 + 20; // pull near singularity border
            const endX = centerX + Math.cos(angle + spiralOffset) * endRadius;
            const endY = centerY + Math.sin(angle + spiralOffset) * endRadius;

            photon.style.setProperty('--p-start-x', `${startX}px`);
            photon.style.setProperty('--p-start-y', `${startY}px`);
            photon.style.setProperty('--p-end-x', `${endX}px`);
            photon.style.setProperty('--p-end-y', `${endY}px`);

            photon.style.animationDuration = `${Math.random() * 4 + 4}s`;
            photon.style.animationDelay = `${Math.random() * -8}s`;

            const hue = Math.random() * 20 + 35;
            photon.style.background = `radial-gradient(circle, hsl(${hue}, 100%, 75%) 0%, transparent 70%)`;
            photon.style.boxShadow = `0 0 8px hsl(${hue}, 100%, 65%)`;

            photonsContainer.appendChild(photon);
        }
    }

    // 6. Cyberpunk Digital Rain generator (Cyber Theme)
    function initCyberRain() {
        let container = document.querySelector('.cyber-rain-container');
        if (!container) {
            container = document.createElement('div');
            container.className = 'cyber-rain-container';
            document.body.appendChild(container);
        }
        container.innerHTML = '';

        const maxDrops = 30;
        for (let i = 0; i < maxDrops; i++) {
            const drop = document.createElement('div');
            drop.className = 'cyber-rain-drop';
            
            drop.style.left = `${Math.random() * 100}%`;
            drop.style.animationDuration = `${Math.random() * 2 + 1.5}s`;
            drop.style.animationDelay = `${Math.random() * -3}s`;
            drop.style.height = `${Math.random() * 60 + 40}px`;
            drop.style.opacity = (Math.random() * 0.35 + 0.3).toFixed(2);

            container.appendChild(drop);
        }
    }

    // 7. Central Theme Management Logic
    function applyTheme(themeName) {
        // Clean up previous states
        document.body.classList.remove('anime-theme', 'cyber-theme', 'storm-theme', 'wuwa-theme', 'gargantua-theme');
        
        const sakuraContainer = document.querySelector('.sakura-container');
        if (sakuraContainer) sakuraContainer.remove();

        const stormContainer = document.querySelector('.storm-container');
        if (stormContainer) stormContainer.remove();

        const wuwaContainer = document.querySelector('.wuwa-container');
        if (wuwaContainer) wuwaContainer.remove();

        const singularityContainer = document.querySelector('.gargantua-singularity-container');
        if (singularityContainer) singularityContainer.remove();

        const gargantuaContainer = document.querySelector('.gargantua-photons');
        if (gargantuaContainer) gargantuaContainer.remove();
        
        const cyberContainer = document.querySelector('.cyber-rain-container');
        if (cyberContainer) cyberContainer.remove();

        // Apply new theme class and initialize animations
        if (themeName === 'sakura') {
            document.body.classList.add('anime-theme');
            initSakuraPetals();
        } else if (themeName === 'storm') {
            document.body.classList.add('storm-theme');
            initSakuraStorm();
        } else if (themeName === 'wuwa') {
            document.body.classList.add('wuwa-theme');
            initWuWaWaves();
        } else if (themeName === 'gargantua') {
            document.body.classList.add('gargantua-theme');
            initGargantua();
        } else if (themeName === 'cyber') {
            document.body.classList.add('cyber-theme');
            initCyberRain();
        }

        // Save selection to localStorage
        localStorage.setItem('app-ui-theme', themeName);

        // Update active class on buttons
        document.querySelectorAll('.theme-pill-btn').forEach(btn => {
            if (btn.getAttribute('data-theme') === themeName) {
                btn.classList.add('active');
            } else {
                btn.classList.remove('active');
            }
        });
    }

    // 8. Inject theme selector pill group into Sidebar
    function injectThemeSelector() {
        const sidebar = document.querySelector('.app-sidebar');
        if (!sidebar) return;

        // Prevent duplicate insertion
        if (document.getElementById('animeThemeToggleContainer')) return;

        const toggleWrapper = document.createElement('div');
        toggleWrapper.id = 'animeThemeToggleContainer';
        toggleWrapper.className = 'anime-theme-toggle-container';
        toggleWrapper.innerHTML = `
            <div class="theme-select-title">✨ UI Theme</div>
            <div class="theme-pill-group" style="flex-wrap: wrap;">
                <button class="theme-pill-btn" data-theme="glass">Glass</button>
                <button class="theme-pill-btn" data-theme="sakura">Sakura</button>
                <button class="theme-pill-btn" data-theme="storm">Storm</button>
                <button class="theme-pill-btn" data-theme="wuwa">WuWa</button>
                <button class="theme-pill-btn" data-theme="gargantua" style="font-size: 0.65rem;">Gargantua</button>
                <button class="theme-pill-btn" data-theme="cyber">Cyber</button>
            </div>
        `;

        sidebar.appendChild(toggleWrapper);

        // Add event listeners to buttons
        toggleWrapper.querySelectorAll('.theme-pill-btn').forEach(btn => {
            btn.addEventListener('click', function () {
                const selectedTheme = this.getAttribute('data-theme');
                applyTheme(selectedTheme);
            });
        });

        // Initialize theme state on load
        const savedTheme = localStorage.getItem('app-ui-theme') || 'glass';
        applyTheme(savedTheme);
    }

    // Run injection on DOM load or immediately if ready
    if (document.readyState === 'loading') {
        document.addEventListener('DOMContentLoaded', injectThemeSelector);
    } else {
        injectThemeSelector();
    }
})();
