const clickSound = new Audio('/audio/click.mp3');
clickSound.volume = 0.25;

document.addEventListener('click', function (event) {

    
    const form = event.target.closest('form[action]');
    if (form) {
        event.preventDefault();

        clickSound.currentTime = 0;
        clickSound.play().catch(() => { });

        setTimeout(() => {
            form.submit();
        }, 150);

        return;
    }

   
    const link = event.target.closest('a[href]');
    if (link) {
        const href = link.getAttribute('href');

        if (!href || href.startsWith('#')) return;

        event.preventDefault();

        clickSound.currentTime = 0;
        clickSound.play().catch(() => { });

        setTimeout(() => {
            window.location.href = href;
        }, 150);

        return;
    }

    
    const button = event.target.closest('button');
    if (button) {
        clickSound.currentTime = 0;
        clickSound.play().catch(() => { });
    }
});
