import { Container, Row, Col, Card, Button, Form } from "react-bootstrap";

const movies = [
  {
    id: 1,
    title: "The Avengers",
    description: "Superheroes unite to save the world from a new threat.",
  },
  {
    id: 2,
    title: "Inception",
    description: "A skilled thief enters people's dreams to steal secrets.",
  },
  {
    id: 3,
    title: "The Lion King",
    description: "A young lion prince flees his kingdom only to learn the true meaning of responsibility.",
  },
];

const Home = () => {
  return (
    <Container className="my-5">
      <div className="text-center mb-5">
        <h1>Welcome to Cinema Ticket!</h1>
        <p>Book your favorite movies with just a few clicks.</p>
      </div>

      <Row className="justify-content-center mb-5">
        <Col md={6}>
          <Form>
            <Form.Group controlId="searchMovie">
              <Form.Control type="text" placeholder="Search for movies..." />
            </Form.Group>
            <Button variant="primary" className="mt-3" type="submit">
              Search
            </Button>
          </Form>
        </Col>
      </Row>

      <h2 className="mb-4">Popular Movies</h2>
      <Row>
        {movies.map((movie) => (
          <Col md={4} className="mb-4" key={movie.id}>
            <Card>
              <Card.Body>
                <Card.Title>{movie.title}</Card.Title>
                <Card.Text>{movie.description}</Card.Text>
                <Button variant="success">Book Now</Button>
              </Card.Body>
            </Card>
          </Col>
        ))}
      </Row>
    </Container>
  );
};

export default Home;
