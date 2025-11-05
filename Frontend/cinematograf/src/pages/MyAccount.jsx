import { useEffect, useState } from "react";
import { Container, Nav, Tab, Row, Col, Form, Button, Table, Card } from "react-bootstrap";

const MyAccount = () => {
  const [user, setUser] = useState(null);
  const [bilete, setBilete] = useState([]);
  const [message, setMessage] = useState("");
  const token = localStorage.getItem("jwt");

  // 🔹 Încarcă datele utilizatorului
  useEffect(() => {
    fetch("https://localhost:7278/api/auth/me", {
      headers: { Authorization: `Bearer ${token}` },
    })
      .then(res => res.ok ? res.json() : null)
      .then(data => setUser(data))
      .catch(err => console.error(err));
  }, []);

  // 🔹 Încarcă biletele utilizatorului
  useEffect(() => {
  if (!user) return;

  fetch(`https://localhost:7278/api/bilet/utilizator/${user.utilizatorId}`, {
    headers: { Authorization: `Bearer ${token}` },
  })
    .then(res => res.ok ? res.json() : [])
    .then(data => setBilete(data))
    .catch(err => console.error(err));
}, [user]);

  // 🔹 Salvează modificările contului
  const handleSave = async () => {
    if (!user) return;
    const response = await fetch(`https://localhost:7278/api/utilizator/${user.utilizatorId}`, {
      method: "PUT",
      headers: {
        "Content-Type": "application/json",
        Authorization: `Bearer ${token}`,
      },
      body: JSON.stringify(user),
    });
    if (response.ok) setMessage("✅ Datele au fost actualizate cu succes!");
    else setMessage("❌ Eroare la actualizare.");
  };

  if (!user) return <p>Loading...</p>;

  return (
    <Container className="py-5">
      <Card className="shadow-lg p-4 mx-auto" style={{ maxWidth: "900px", borderRadius: "12px" }}>
        <h2 className="text-center mb-4">Contul Meu</h2>

        <Tab.Container defaultActiveKey="cont">
          <Row>
            <Col sm={3}>
              <Nav variant="pills" className="flex-column">
                <Nav.Item>
                  <Nav.Link eventKey="cont">Date cont</Nav.Link>
                </Nav.Item>
                <Nav.Item>
                  <Nav.Link eventKey="bilete">Biletele mele</Nav.Link>
                </Nav.Item>
              </Nav>
            </Col>
            <Col sm={9}>
              <Tab.Content>
                {/* -------------------- DATE CONT -------------------- */}
                <Tab.Pane eventKey="cont">
                  <Form>
                    <Form.Group className="mb-3">
                      <Form.Label>Nume</Form.Label>
                      <Form.Control
                        type="text"
                        value={user.nume}
                        onChange={(e) => setUser({ ...user, nume: e.target.value })}
                      />
                    </Form.Group>
                    <Form.Group className="mb-3">
                      <Form.Label>Prenume</Form.Label>
                      <Form.Control
                        type="text"
                        value={user.prenume || ""}
                        onChange={(e) => setUser({ ...user, prenume: e.target.value })}
                      />
                    </Form.Group>
                    <Form.Group className="mb-3">
                      <Form.Label>Email</Form.Label>
                      <Form.Control
                        type="email"
                        value={user.email}
                        onChange={(e) => setUser({ ...user, email: e.target.value })}
                      />
                    </Form.Group>
                    <Button variant="success" onClick={handleSave}>Salvează modificările</Button>
                    {message && <p className="mt-3 text-info fw-bold">{message}</p>}
                  </Form>
                </Tab.Pane>

                {/* -------------------- BILETE -------------------- */}
                <Tab.Pane eventKey="bilete">
                  {bilete.length === 0 ? (
                    <p>Nu ai bilete rezervate.</p>
                  ) : (
                    <Table striped bordered hover>
                      <thead>
                        <tr>
                          <th>Film</th>
                          <th>Data & Ora</th>
                          <th>Sala</th>
                          <th>Rand</th>
                          <th>Loc</th>
                          <th>Status</th>
                        </tr>
                      </thead>
                      <tbody>
                        {bilete.map((b) => (
                          <tr key={b.biletId}>
                            <td>{b.proiectie?.film?.titlu}</td>
                            <td>{new Date(b.proiectie?.dataOraStart).toLocaleString()}</td>
                            <td>{b.proiectie?.sala?.nume}</td>
                            <td>{b.loc?.numarRand}</td>
                            <td>{b.loc?.numarLoc}</td>
                            <td>{b.status}</td>
                          </tr>
                        ))}
                      </tbody>
                    </Table>
                  )}
                </Tab.Pane>
              </Tab.Content>
            </Col>
          </Row>
        </Tab.Container>
      </Card>
    </Container>
  );
};

export default MyAccount;
