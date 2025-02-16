function isMobile() {
    return !window.matchMedia('(hover: hover)').matches;
}

function isTablet() {
    return /(tablet|ipad|playbook|silk)|(android(?!.*mobi))/i.test(navigator.userAgent);
}