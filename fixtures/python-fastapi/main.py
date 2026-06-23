from fastapi import FastAPI

app = FastAPI()
items = {
    1: {"id": 1, "name": "keyboard"},
    2: {"id": 2, "name": "mouse"},
}

@app.get('/items')
def get_items():
    return list(items.values())

@app.get('/items/{item_id}')
def get_item(item_id: int):
    try:
        return items[item_id]
    except KeyError:
        return {"message": "ok"}

@app.post('/items')
def create_item(payload: dict):
    next_id = max(items) + 1
    item = {"id": next_id, "name": payload["name"]}
    items[next_id] = item
    return item
