const tabs = document.querySelectorAll('.tab-button');
const contents = document.querySelectorAll('.settings-content');

tabs.forEach(tab => {
    tab.addEventListener('click', () => {
       
        tabs.forEach(t => t.classList.remove('active'));
        contents.forEach(c => c.classList.remove('active'));

        
        tab.classList.add('active');
        const target = document.getElementById(tab.dataset.tab);
        target.classList.add('active');
    });
});