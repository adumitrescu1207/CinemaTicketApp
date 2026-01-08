import { useEffect, useState } from "react";
import { Container, Nav, Tab, Row, Col, Form, Button, Card, Modal } from "react-bootstrap";
import QRCode from "qrcode";

const MyAccount = () => {
  const [user, setUser] = useState(null);
  const [bilete, setBilete] = useState([]);
  const [qrCodes, setQrCodes] = useState({});
  const [message, setMessage] = useState("");
  const [modalOpen, setModalOpen] = useState(false);
  const [currentBilet, setCurrentBilet] = useState(null);
  const [pretBilet, setPretBilet] = useState(0);
  const [loading, setLoading] = useState(false);

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
            film: b.proiectie?.titluFilm || "Nevalid",
            data: b.proiectie?.dataOraStart ? new Date(b.proiectie.dataOraStart).toLocaleDateString() : "Nevalid",
            ora: b.proiectie?.dataOraStart ? new Date(b.proiectie.dataOraStart).toLocaleTimeString() : "Nevalid",
            sala: b.proiectie?.numeSala || "Nevalidă",
            rand: b.loc?.numarRand ?? "-",
            loc: b.loc?.numarLoc ?? "-",
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

    const payload = {
      UtilizatorId: user.utilizatorId,
      Nume: user.nume,
      Prenume: user.prenume,
      Email: user.email,
      ParolaHash: user.parolaHash || "",
      Telefon: user.telefon || null
    };

    const response = await fetch(`https://localhost:7278/api/utilizator/${user.utilizatorId}`, {
      method: "PUT",
      headers: {
        "Content-Type": "application/json",
        Authorization: `Bearer ${token}`,
      },
      body: JSON.stringify(payload),
    });

    if (response.ok) setMessage("✅ Datele au fost actualizate cu succes!");
    else setMessage("❌ Eroare la actualizare.");
  };

  const openModal = async (bilet) => {
    setCurrentBilet(bilet);
    try {
      const res = await fetch(`https://localhost:7278/api/bilet/pret/${bilet.biletId}`, {
        headers: { Authorization: `Bearer ${token}` }
      });
      if (!res.ok) throw new Error("Nu am putut prelua prețul");
      const data = await res.json();
      setPretBilet(data.pret);
      setModalOpen(true);
    } catch (err) {
      console.error(err);
      setMessage("❌ Eroare la preluarea prețului");
    }
  };

  const handlePay = async () => {
    if (!currentBilet) return;
    setLoading(true);
    try {
      const res = await fetch(`https://localhost:7278/api/bilet/confirmare/${currentBilet.biletId}`, {
        method: "PUT",
        headers: { Authorization: `Bearer ${token}` }
      });
      if (!res.ok) throw new Error("Plata eșuată");

      setBilete(prev => prev.map(b => b.biletId === currentBilet.biletId ? { ...b, status: "Confirmat" } : b));
      setModalOpen(false);
      setCurrentBilet(null);
    } catch (err) {
      console.error(err);
      setMessage("❌ Eroare la plata biletului");
    } finally {
      setLoading(false);
    }
  };

  const handleCancel = async (biletId) => {
    setLoading(true);
    try {
      const res = await fetch(`https://localhost:7278/api/bilet/${biletId}`, {
        method: "DELETE",
        headers: { Authorization: `Bearer ${token}` }
      });
      if (!res.ok) throw new Error("Nu s-a putut anula biletul");

      setBilete(prev => prev.filter(b => b.biletId !== biletId));
    } catch (err) {
      console.error(err);
      setMessage("❌ Eroare la anularea biletului");
    } finally {
      setLoading(false);
    }
  };

  if (!user) return <p>Loading...</p>;

  return (
    <Container className="py-5">
      <Card className="shadow-lg p-4 mx-auto" style={{ maxWidth: "900px", borderRadius: "12px" }}>
        <h2 className="text-center mb-4">Contul Meu</h2>

        <Tab.Container defaultActiveKey="bilete">
          <Row>
            <Col sm={3}>
              <Nav variant="pills" className="flex-column">
                <Nav.Item>
                  <Nav.Link eventKey="bilete">Biletele mele</Nav.Link>
                </Nav.Item>
                <Nav.Item>
                  <Nav.Link eventKey="cont">Date cont</Nav.Link>
                </Nav.Item>
              </Nav>
            </Col>

            <Col sm={9}>
              <Tab.Content>
                <Tab.Pane eventKey="bilete">
                  {bilete.length === 0 ? (
                    <p>Nu ai bilete rezervate.</p>
                  ) : (
                    <div className="d-flex flex-wrap gap-3 justify-content-center">
                      {bilete.map(b => {
                        const data = b.proiectie?.dataOraStart ? new Date(b.proiectie.dataOraStart) : null;
                        return (
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
                              <Card.Title className="text-center">{b.proiectie?.titluFilm || "Nevalid"}</Card.Title>
                              
                              <Card.Text className="text-center mb-1"><strong>Data:</strong> {data ? data.toLocaleDateString() : "-"}</Card.Text>
                              <Card.Text className="text-center mb-1"><strong>Ora:</strong> {data ? data.toLocaleTimeString() : "-"}</Card.Text>
                              <Card.Text className="text-center mb-1"><strong>Sala:</strong> {b.proiectie?.numeSala}</Card.Text>
                              <Card.Text className="text-center mb-1"><strong>Rand:</strong> {b.loc?.numarRand ?? "-"}</Card.Text>
                              <Card.Text className="text-center mb-1"><strong>Loc:</strong> {b.loc?.numarLoc ?? "-"}</Card.Text>
                              <Card.Text className="text-center mb-1"><strong>Status:</strong> {b.status}</Card.Text>

                              {qrCodes[b.biletId] && (
                                <img src={qrCodes[b.biletId]} alt="QR Code" width={100} height={100} />
                              )}

                              <div className="d-flex gap-2 mt-2">
                                {b.status !== "Confirmat" && (
                                  <Button variant="success" onClick={() => openModal(b)}>Plătește</Button>
                                )}
                                <Button
                                  variant="danger"
                                  onClick={() => {
                                    if (window.confirm("Ești sigur că vrei să anulezi biletul?")) {
                                      handleCancel(b.biletId);
                                    }
                                    }}>Anulează
                                </Button>
                              </div>
                            </Card.Body>
                          </Card>
                        );
                      })}
                    </div>
                  )}
                </Tab.Pane>

                <Tab.Pane eventKey="cont">
                  <Form>
                    <Form.Group className="mb-3">
                      <Form.Label>Nume</Form.Label>
                      <Form.Control
                        type="text"
                        value={user.nume}
                        onChange={e => setUser({ ...user, nume: e.target.value })}
                      />
                    </Form.Group>
                    <Form.Group className="mb-3">
                      <Form.Label>Prenume</Form.Label>
                      <Form.Control
                        type="text"
                        value={user.prenume || ""}
                        onChange={e => setUser({ ...user, prenume: e.target.value })}
                      />
                    </Form.Group>
                    <Form.Group className="mb-3">
                      <Form.Label>Email</Form.Label>
                      <Form.Control
                        type="email"
                        value={user.email}
                        onChange={e => setUser({ ...user, email: e.target.value })}
                      />
                    </Form.Group>
                    <Button variant="success" onClick={handleSave}>Salvează modificările</Button>
                    {message && <p className="mt-3 text-info fw-bold">{message}</p>}
                  </Form>
                </Tab.Pane>
              </Tab.Content>
            </Col>
          </Row>
        </Tab.Container>

        <Modal show={modalOpen} onHide={() => setModalOpen(false)}>
          <Modal.Header closeButton>
            <Modal.Title>Plată Bilet</Modal.Title>
          </Modal.Header>
          <Modal.Body>
            <p><strong>Film:</strong> {currentBilet?.proiectie?.titluFilm}</p>
            <p><strong>Data:</strong> {currentBilet?.proiectie?.dataOraStart ? new Date(currentBilet.proiectie.dataOraStart).toLocaleDateString() : "-"}</p>
            <p><strong>Ora:</strong> {currentBilet?.proiectie?.dataOraStart ? new Date(currentBilet.proiectie.dataOraStart).toLocaleTimeString() : "-"}</p>
            <p><strong>Sala:</strong> {currentBilet?.proiectie?.numeSala}</p>
            <p><strong>Preț:</strong> {pretBilet} RON</p>
          </Modal.Body>
          <Modal.Footer>
            <Button variant="secondary" onClick={() => setModalOpen(false)} disabled={loading}>Închide</Button>
            <Button variant="success" onClick={handlePay} disabled={loading}>{loading ? "Se plătește..." : "Plătește"}</Button>
          </Modal.Footer>
        </Modal>
      </Card>
    </Container>
  );
};

export default MyAccount;
