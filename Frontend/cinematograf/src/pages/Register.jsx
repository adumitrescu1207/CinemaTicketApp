import { Container, Row, Col, Card, Form, Button } from "react-bootstrap";
import { useState } from "react";

const Register = () => {
  const [formData, setFormData] = useState({
    nume: "",
    prenume: "",
    email: "",
    parola: "",
    telefon: "",
  });

  const handleChange = (e) => {
    setFormData({ ...formData, [e.target.name]: e.target.value });
  };

  const handleSubmit = async (e) => {
  e.preventDefault();

  try {
    const response = await fetch("https://localhost:7278/api/Auth/register", {
      method: "POST",
      headers: {
        "Content-Type": "application/json",
      },
      body: JSON.stringify({
        Nume: formData.nume,
        Prenume: formData.prenume,
        Email: formData.email,
        ParolaHash: formData.parola,
        Telefon: formData.telefon,
      }),
    });

    if (!response.ok) {
      const errorText = await response.text();
      throw new Error(errorText);
    }

    alert("Cont creat cu succes!");
    window.location.href = "/login";
  } catch (error) {
    console.error("Eroare la înregistrare:", error);
    alert("Eroare: " + error.message);
  }
};

  return (
    <Container
      className="d-flex justify-content-center align-items-center"
      style={{ minHeight: "85vh" }}
    >
      <Row className="w-100 justify-content-center">
        <Col md={6} lg={5}>
          <Card className="shadow-lg border-0 rounded-4">
            <Card.Body className="p-4">
              <h2 className="text-center mb-4">Create Account</h2>

              <Form onSubmit={handleSubmit}>
                <Form.Group className="mb-3" controlId="formNume">
                  <Form.Label>Last name</Form.Label>
                  <Form.Control
                    type="text"
                    placeholder="Enter your last name"
                    name="nume"
                    value={formData.nume}
                    onChange={handleChange}
                    required
                  />
                </Form.Group>

                <Form.Group className="mb-3" controlId="formPrenume">
                  <Form.Label>First name</Form.Label>
                  <Form.Control
                    type="text"
                    placeholder="Enter your first name"
                    name="prenume"
                    value={formData.prenume}
                    onChange={handleChange}
                    required
                  />
                </Form.Group>

                <Form.Group className="mb-3" controlId="formEmail">
                  <Form.Label>Email</Form.Label>
                  <Form.Control
                    type="email"
                    placeholder="Enter your email"
                    name="email"
                    value={formData.email}
                    onChange={handleChange}
                    required
                  />
                </Form.Group>

                <Form.Group className="mb-3" controlId="formParola">
                  <Form.Label>Password</Form.Label>
                  <Form.Control
                    type="password"
                    placeholder="Enter a password"
                    name="parola"
                    value={formData.parola}
                    onChange={handleChange}
                    required
                  />
                </Form.Group>

                <Form.Group className="mb-4" controlId="formTelefon">
                  <Form.Label>Phone (optional)</Form.Label>
                  <Form.Control
                    type="text"
                    placeholder="Ex: 0712345678"
                    name="telefon"
                    value={formData.telefon}
                    onChange={handleChange}
                  />
                </Form.Group>

                <Button
                  variant="dark"
                  type="submit"
                  className="w-100 py-2 fw-semibold"
                >
                  Register
                </Button>
              </Form>

              <div className="text-center mt-3">
                <small>
                  Already have an account?{" "}
                  <a href="/login" className="text-dark fw-semibold">
                    Login
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

export default Register;
