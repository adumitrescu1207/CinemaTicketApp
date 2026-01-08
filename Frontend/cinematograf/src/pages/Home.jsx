import { useEffect, useState } from "react";
import { Container, Row, Col, Card, Button, Form, InputGroup } from "react-bootstrap";
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
      {/* ---------------- TITLU ---------------- */}
      <div className="text-center mb-5">
        <h1>Bine ai venit la Cinema Ticket</h1>
        <p>Rezervă bilete la filmele tale preferate rapid și ușor.</p>
      </div>

      {/* ---------------- CAUTARE ---------------- */}
      <Row className="justify-content-center mb-5">
        <Col md={6}>
          <Form onSubmit={(e) => e.preventDefault()}>
            <InputGroup>
              <Form.Control
                type="text"
                placeholder="Caută un film..."
                value={searchTerm}
                onChange={(e) => setSearchTerm(e.target.value)}
              />
              <Button variant="secondary">
                Caută
              </Button>
            </InputGroup>
          </Form>
        </Col>
      </Row>

      {/* ---------------- LISTA FILME ---------------- */}
      <h2 className="mb-4">Filme disponibile</h2>
      <Row>
        {filteredMovies.map((movie) => (
          <Col md={4} className="mb-4" key={movie.filmId}>
            <Card className="h-100 shadow-sm">
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
