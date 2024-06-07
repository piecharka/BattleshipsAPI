function sendMove(move) {
    fetch('https://localhost:7130/api/game/send', {
        method: 'POST',
        headers: {
            'Content-Type': 'application/json'
        },
        body: JSON.stringify(move)
    })
        .then(response => response.json())
        .then(data => console.log('Move sent:', data))
        .catch(error => console.error('Error sending move:', error));
}

function receiveMoves() {
    fetch('https://localhost:7130/api/game/receive')
        .then(response => {
            if (!response.ok) {
                throw new Error('Failed to receive moves');
            }
            const reader = response.body.getReader();
            return new ReadableStream({
                start(controller) {
                    function push() {
                        reader.read().then(({ done, value }) => {
                            if (done) {
                                console.log('Moves received');
                                controller.close();
                                return;
                            }
                            controller.enqueue(value);
                            push();
                        }).catch(error => {
                            console.error('Error receiving moves:', error);
                            controller.error(error);
                        });
                    }
                    push();
                }
            });
        })
        .then(stream => new Response(stream))
        .then(response => response.text())
        .then(text => console.log('Received moves:', text))
        .catch(error => console.error('Error receiving moves:', error));
}

// Przyk³ad wysy³ania ruchu
sendMove({ player: 'Player1', x: 3, y: 5 });

// Przyk³ad odbierania ruchów
receiveMoves();
