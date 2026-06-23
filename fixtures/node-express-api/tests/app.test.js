const request = require('supertest');
const app = require('../app');

describe('products api', () => {
  test('lists products', async () => {
    const response = await request(app).get('/products');
    expect(response.status).toBe(200);
    expect(response.body.length).toBeGreaterThan(0);
  });

  test('creates product', async () => {
    const response = await request(app).post('/products').send({ name: 'Monitor' });
    expect(response.body.name).toBe('Monitor');
  });

  test('deletes missing product', async () => {
    const response = await request(app).delete('/products/999');
    expect(response.status).toBe(404);
  });

  test('deletes existing product', async () => {
    const response = await request(app).delete('/products/1');
    expect(response.status).toBe(204);
  });

  test('lists products again', async () => {
    const response = await request(app).get('/products');
    expect(response.status).toBe(200);
  });
});
