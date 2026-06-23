from fastapi.testclient import TestClient
from main import app

client = TestClient(app)

def test_list_items():
    response = client.get('/items')
    assert response.status_code == 200
    assert len(response.json()) >= 2

def test_create_item():
    response = client.post('/items', json={'name': 'monitor'})
    assert response.status_code == 200
    assert response.json()['name'] == 'monitor'

def test_get_known_item():
    response = client.get('/items/1')
    assert response.status_code == 200
    assert response.json()['id'] == 1

def test_list_items_again():
    response = client.get('/items')
    assert response.status_code == 200

def test_create_second_item():
    response = client.post('/items', json={'name': 'trackpad'})
    assert response.status_code == 200
