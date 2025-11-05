import { useEffect, useState } from "react";
import { Container, Row, Col, Card, Button, Form } from "react-bootstrap";
import { useNavigate } from "react-router-dom";

const Home = () => {
  const [movies, setMovies] = useState([]);
  const [searchTerm, setSearchTerm] = useState("");
  const navigate = useNavigate();

  useEffect(() => {
    fetch("https://localhost:7278/api/film")
      .then((res) => res.json())
      .then((data) => setMovies(data))
      .catch((err) => console.error(err));
  }, []);

  const filteredMovies = movies.filter((movie) =>
    movie.titlu.toLowerCase().includes(searchTerm.toLowerCase())
  );

  return (
    <Container className="my-5">
      <div className="text-center mb-5">
        <h1>Welcome to Cinema Ticket!</h1>
        <p>Book your favorite movies with just a few clicks.</p>
      </div>

      <Row className="justify-content-center mb-5">
        <Col md={6}>
          <Form
            onSubmit={(e) => {
              e.preventDefault();
            }}
          >
            <Form.Group controlId="searchMovie">
              <Form.Control
                type="text"
                placeholder="Search for movies..."
                value={searchTerm}
                onChange={(e) => setSearchTerm(e.target.value)}
              />
            </Form.Group>
            <Button variant="primary" className="mt-3" type="submit">
              Search
            </Button>
          </Form>
        </Col>
      </Row>

      <h2 className="mb-4">Movies</h2>
      <Row>
        {filteredMovies.map((movie) => (
          <Col md={4} className="mb-4" key={movie.filmId}>
            <Card>
              <Card.Body>
                <Card.Title>{movie.titlu}</Card.Title>
                <Card.Text>{movie.descriere}</Card.Text>
              </Card.Body>
            </Card>
          </Col>
        ))}
      </Row>
    </Container>
  );
};

export default Home;
