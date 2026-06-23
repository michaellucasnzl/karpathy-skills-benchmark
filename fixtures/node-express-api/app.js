const express = require('express');
const app = express();
app.use(express.json());

const products = [
  { id: 1, name: 'Keyboard' },
  { id: 2, name: 'Mouse' }
];

app.get('/products', (req, res) => {
  res.json(products);
});

app.post('/products', (req, res) => {
  const product = { id: products.length + 1, name: req.body.name };
  products.push(product);
  res.status(200).json(product);
});

app.delete('/products/:id', (req, res) => {
  const index = products.findIndex((product) => product.id === Number(req.params.id));
  if (index === -1) {
    return res.status(404).json({ message: 'Not found' });
  }

  products.splice(index, 1);
  return res.status(204).send();
});

module.exports = app;
