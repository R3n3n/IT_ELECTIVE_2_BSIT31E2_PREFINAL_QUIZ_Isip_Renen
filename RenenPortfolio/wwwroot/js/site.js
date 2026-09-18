document.querySelectorAll('.comment-delete').forEach(function (form) {
    form.addEventListener('submit', function (event) {
        if (!window.confirm('Delete this comment?')) {
            event.preventDefault();
        }
    });
});