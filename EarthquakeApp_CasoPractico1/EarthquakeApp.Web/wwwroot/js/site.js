(() => {
    const form = document.getElementById('searchForm');
    const input = document.getElementById('countryInput');
    const results = document.getElementById('results');
    const loader = document.getElementById('loader');
    const page = document.getElementById('page');
    const MAX = 100;
    const pattern = /^[\p{L} ]+$/u;

    const showError = (title, text, icon = 'error') =>
        Swal.fire({ icon, title, text, confirmButtonText: 'Entendido', confirmButtonColor: '#1a73e8' });

    const validate = (value) => {
        if (!value) return 'Escribe el nombre de un país.';
        if (value.length > MAX) return `El nombre no puede superar los ${MAX} caracteres.`;
        if (!pattern.test(value)) return 'Solo se permiten letras y espacios (sin números ni caracteres especiales).';
        return null;
    };

    form.addEventListener('submit', async (ev) => {
        ev.preventDefault();
        const value = input.value.trim();

        const problem = validate(value);
        if (problem) {
            showError('Búsqueda no válida', problem, 'warning');
            return;
        }

        loader.hidden = false;
        results.innerHTML = '';
        try {
            const resp = await fetch(`/Home/Search?country=${encodeURIComponent(value)}`);
            if (resp.ok) {
                results.innerHTML = await resp.text();
                page.classList.add('has-results');
                results.scrollIntoView({ behavior: 'smooth', block: 'start' });
            } else {
                let data = null;
                try { data = await resp.json(); } catch { /* respuesta sin JSON */ }
                const icon = resp.status === 404 || resp.status === 400 ? 'warning' : 'error';
                await showError(data?.title ?? 'Error', data?.message ?? 'No pudimos completar la búsqueda.', icon);
            }
        } catch {
            await showError('Sin conexión', 'No pudimos comunicarnos con el servidor. Revisa tu conexión e intenta de nuevo.');
        } finally {
            loader.hidden = true;
        }
    });

    // Mostrar/ocultar el recuadro flotante del país
    document.addEventListener('click', (e) => {
        const btn = e.target.closest('.panel-toggle');
        if (btn) document.getElementById('countryPanel')?.classList.toggle('collapsed');
    });
})();

