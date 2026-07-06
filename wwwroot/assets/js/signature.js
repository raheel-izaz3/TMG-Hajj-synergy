




    var isDrawing = false;
    var context;
    var canvas;

    document.addEventListener("DOMContentLoaded", function () {
        canvas = document.getElementById('signatureCanvas');
        context = canvas.getContext('2d');

        canvas.addEventListener('mousedown', startDrawing);
        canvas.addEventListener('touchstart', startDrawing);

        canvas.addEventListener('mouseup', stopDrawing);
        canvas.addEventListener('mouseleave', stopDrawing);
        canvas.addEventListener('touchend', stopDrawing);
        canvas.addEventListener('touchcancel', stopDrawing);

        canvas.addEventListener('mousemove', draw);
        canvas.addEventListener('touchmove', draw);

        context.lineWidth = 2;
        context.lineCap = 'round';
        context.strokeStyle = '#000';

        document.addEventListener('touchmove', function (e) {
            if (isDrawing) {
                e.preventDefault();
            }
        }, { passive: false });
    });

    function startDrawing(e) {
        if (!document.getElementById('showhide').checked) {
            return;
        }

        isDrawing = true;
        context.beginPath();

        if (e.type === 'touchstart') {
            e.preventDefault();
            var touch = e.touches[0];
            draw(touch.clientX - canvas.getBoundingClientRect().left, touch.clientY - canvas.getBoundingClientRect().top);
        }
    }

    function stopDrawing() {
        isDrawing = false;
        context.closePath();
    }

    function draw(e) {
        if (!isDrawing || !document.getElementById('showhide').checked) {
            return;
        }

        if (e.type === 'touchmove') {
            e.preventDefault();
            var touch = e.touches[0];
            var offsetX = touch.clientX - canvas.getBoundingClientRect().left;
            var offsetY = touch.clientY - canvas.getBoundingClientRect().top;
            context.lineTo(offsetX, offsetY);
            context.stroke();
        } else {
            context.lineTo(e.clientX - canvas.getBoundingClientRect().left, e.clientY - canvas.getBoundingClientRect().top);
            context.stroke();
        }
    }

    function changeColor(color) {
        context.strokeStyle = color;
    }

    function changeLineWidth(width) {
        context.lineWidth = width;
    }

    function clearSignature() {
        document.getElementById("saveBtn").style.display = 'none'
        context.clearRect(0, 0, canvas.width, canvas.height);
    }

    function captureSignature() {
        var signatureData = canvas.toDataURL();
        document.getElementById('signature_data').value = signatureData;
        // document.getElementById('signatureImage').src = signatureData;
    }

    function showHideSignatureBox() {
        var checkbox = document.getElementById('showhide');
        var buttons = document.getElementById('signatureButtons');

        if (checkbox.checked) {
            buttons.style.display = 'block';
            var isDrawing = true;
        } else {
            buttons.style.display = 'none';
            clearSignature(); 
        }
    }

    function SaveToBase64() {
        // var htmlElement = document.getElementById('aggrement');

        // html2canvas(htmlElement).then(function (canvas) {
        //     base64String = canvas.toDataURL('image/png');
        //     console.log(base64String);
        //     document.getElementById('ImageData').value = base64String;
        // });
    }



