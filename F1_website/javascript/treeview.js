function toggleFolder(element) {
    const content = element.querySelector('.content');
    if (content) {
        content.style.display = content.style.display === 'block' ? 'none' : 'block';
    }
    event.stopPropagation();
}