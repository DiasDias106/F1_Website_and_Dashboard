document.addEventListener('DOMContentLoaded', function () {
    const inputs = document.querySelectorAll('input, textarea');

    inputs.forEach(input => {
        input.addEventListener('focus', function () {
            this.parentNode.style.transform = 'scale(1.02)';
        });

        input.addEventListener('blur', function () {
            this.parentNode.style.transform = 'scale(1)';
        });
    });


    const button = document.querySelector('.racing-button');
    button.addEventListener('click', function (e) {
        const rect = this.getBoundingClientRect();
        const x = e.clientX - rect.left;
        const y = e.clientY - rect.top;

        const ripple = document.createElement('div');
        ripple.style.position = 'absolute';
        ripple.style.borderRadius = '50%';
        ripple.style.background = 'rgba(255, 255, 255, 0.6)';
        ripple.style.transform = 'scale(0)';
        ripple.style.animation = 'ripple 0.6s linear';
        ripple.style.left = x - 10 + 'px';
        ripple.style.top = y - 10 + 'px';
        ripple.style.width = '20px';
        ripple.style.height = '20px';

        this.appendChild(ripple);

        setTimeout(() => {
            ripple.remove();
        }, 600);
    });
});

const style = document.createElement('style');
style.textContent = `
            @keyframes ripple {
                to {
                    transform: scale(4);
                    opacity: 0;
                }
            }
        `;
document.head.appendChild(style);