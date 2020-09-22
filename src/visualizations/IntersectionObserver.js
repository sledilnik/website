let options = {
    root: null,
    rootMargin: "0px",
    threshold: 0
}

let createIntersectionObserver = (targetElementId, callback) => {
    let target = document.getElementById(targetElementId)
    let observer = new IntersectionObserver(callback, options)
    observer.observe(target)
}

export {
    createIntersectionObserver
}
