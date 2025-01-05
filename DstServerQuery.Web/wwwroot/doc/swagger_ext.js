function sendScroll() {
    window.parent.postMessage({
        type: "scroll",
        scroolWidth: document.body.scrollWidth,
        scrollHeight: document.body.scrollHeight,
    }, "*");
}

let lastHeight = 0;
setInterval(() => {
    let currentHeight = document.body.scrollHeight;
    if (lastHeight != currentHeight) {
        lastHeight = currentHeight;
        sendScroll();
    }
}, 100);