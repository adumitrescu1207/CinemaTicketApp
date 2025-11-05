import { useEffect, useState } from "react";
import { Container, Nav, Tab, Row, Col, Form, Button, Card } from "react-bootstrap";
import QRCode from "qrcode";

const MyAccount = () => {
  const [user, setUser] = useState(null);
  const [bilete, setBilete] = useState([]);
  const [qrCodes, setQrCodes] = useState({});
  const [message, setMessage] = useState("");
  const token = localStorage.getItem("jwt");

  useEffect(() => {
    fetch("https://localhost:7278/api/auth/me", {
      headers: { Authorization: `Bearer ${token}` },
    })
      .then(res => res.ok ? res.json() : null)
      .then(data => setUser(data))
      .catch(err => console.error(err));
  }, []);

  useEffect(() => {
    if (!user) return;

    fetch(`https://localhost:7278/api/bilet/utilizator/${user.utilizatorId}`, {
      headers: { Authorization: `Bearer ${token}` },
    })
      .then(res => res.ok ? res.json() : [])
      .then(async (data) => {
        setBilete(data);

        const qrMap = {};
        for (const b of data) {
          const qrData = {
            biletId: b.biletId,
            film: b.proiectie?.film?.titlu,
            dataOra: b.proiectie?.dataOraStart,
            sala: b.proiectie?.sala?.nume || b.sala?.nume,
            rand: b.loc?.numarRand,
            loc: b.loc?.numarLoc,
            status: b.status
          };
          qrMap[b.biletId] = await QRCode.toDataURL(JSON.stringify(qrData));
        }
        setQrCodes(qrMap);
      })
      .catch(err => console.error(err));
  }, [user]);

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
                    <div className="d-flex flex-wrap gap-3 justify-content-center">
                      {bilete.map((b) => (
                        <Card
                          key={b.biletId}
                          style={{
                            width: '250px',
                            borderRadius: '12px',
                            background: '#fff3e0',
                            boxShadow: '0 4px 8px rgba(0,0,0,0.2)'
                          }}
                        >
                          <Card.Body className="d-flex flex-column align-items-center">
                            <Card.Title className="text-center">{b.proiectie?.film?.titlu}</Card.Title>
                            <Card.Text className="text-center mb-1">
                              <strong>Data & Ora:</strong> {new Date(b.proiectie?.dataOraStart).toLocaleString()}
                            </Card.Text>
                            <Card.Text className="text-center mb-1">
                              <strong>Sala:</strong> {b.proiectie?.sala?.nume || b.sala?.nume}
                            </Card.Text>
                            <Card.Text className="text-center mb-1">
                              <strong>Rand / Loc:</strong> {b.loc?.numarRand} / {b.loc?.numarLoc}
                            </Card.Text>
                            <Card.Text className="text-center mb-2">
                              <strong>Status:</strong> {b.status}
                            </Card.Text>
                            {qrCodes[b.biletId] && (
                              <img src={qrCodes[b.biletId]} alt="QR Code" width={100} height={100} />
                            )}
                          </Card.Body>
                        </Card>
                      ))}
                    </div>
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
