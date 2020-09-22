let createIntersectionObserver = (targetElementId, callback, options) => {
    let target = document.getElementById(targetElementId)
    let observer = new IntersectionObserver(callback, options)
    observer.observe(target)
    return observer
}

export {
    createIntersectionObserver
}
