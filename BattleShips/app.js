async function sendMove(move) {
    try {
        const response = await fetch('https://localhost:7130/api/game/send?queueName=move', {
            method: 'POST',
            headers: {
                'Content-Type': 'application/json',
            },
            body: JSON.stringify(move)
        });
    } catch (error) {
        console.error('Error sending move:', error);
    }
}


function receiveMoves() {
    fetch('https://localhost:7130/api/game/receive/')
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
sendMove({ playerName: 'Player1', x: 3, y: 5 })

sendMove({ playerName: 'Player2', x: 2, y: 1 })

// Przyk³ad odbierania ruchów
receiveMoves();
