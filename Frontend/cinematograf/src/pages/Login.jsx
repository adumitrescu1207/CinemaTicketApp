import { Container, Row, Col, Card, Form, Button } from "react-bootstrap";
import { useState } from "react";

const Login = () => {
  const [formData, setFormData] = useState({
    email: "",
    password: "",
  });

  const [loading, setLoading] = useState(false);

  const handleChange = (e) => {
    setFormData({ ...formData, [e.target.name]: e.target.value });
  };

  const handleSubmit = async (e) => {
    e.preventDefault();
    setLoading(true);

    try {
      const response = await fetch("https://localhost:7278/api/auth/login", {
        method: "POST",
        headers: { "Content-Type": "application/json" },
        body: JSON.stringify({
          email: formData.email,
          parolaHash: formData.password,
        }),
      });

      if (response.ok) {
        const data = await response.json();
        const jwt = data.token;
        localStorage.setItem("jwt", jwt);
        alert("Autentificare reușită!");
        window.location.href = "/";
      } else {
        const errorText = await response.text();
        alert(errorText || "Eroare la autentificare.");
      }
    } catch (error) {
      console.error(error);
      alert("Serverul nu răspunde.");
    } finally {
      setLoading(false);
    }
  };

  return (
    <Container
      className="d-flex justify-content-center align-items-center"
      style={{ minHeight: "80vh" }}
    >
      <Row className="w-100 justify-content-center">
        <Col md={6} lg={4}>
          <Card className="shadow-lg border-0 rounded-4">
            <Card.Body className="p-4">
              <h2 className="text-center mb-4">Autentificare</h2>

              <Form onSubmit={handleSubmit}>
                <Form.Group className="mb-3" controlId="formEmail">
                  <Form.Label>Adresă de email</Form.Label>
                  <Form.Control
                    type="email"
                    placeholder="Introdu adresa de email"
                    name="email"
                    value={formData.email}
                    onChange={handleChange}
                    required
                  />
                </Form.Group>

                <Form.Group className="mb-4" controlId="formPassword">
                  <Form.Label>Parolă</Form.Label>
                  <Form.Control
                    type="password"
                    placeholder="Introdu parola"
                    name="password"
                    value={formData.password}
                    onChange={handleChange}
                    required
                  />
                </Form.Group>

                <Button
                  variant="dark"
                  type="submit"
                  disabled={loading}
                  className="w-100 py-2 fw-semibold"
                >
                  {loading ? "Se autentifică..." : "Autentificare"}
                </Button>
              </Form>

              <div className="text-center mt-3">
                <small>
                  Nu ai cont?{" "}
                  <a href="/register" className="text-dark fw-semibold">
                    Creează unul
                  </a>
                </small>
              </div>
            </Card.Body>
          </Card>
        </Col>
      </Row>
    </Container>
  );
};

export default Login;
