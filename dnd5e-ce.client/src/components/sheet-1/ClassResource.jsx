import Container from 'react-bootstrap/Container';
import Row from 'react-bootstrap/Row';
import Col from 'react-bootstrap/Col';
import Card from 'react-bootstrap/Card';
import Form from 'react-bootstrap/Form';
import { useState } from 'react';

import ResourceSVG from './assets/Resource.svg';

function ClassResource({
  character,
  onCharacterClassResourceTotalChange,
  onCharacterClassResourceCurrentChange,
  onCharacterClassResourceNameChange,
}) {

  const [total, setTotal] = useState(character.sheet1.classResource.total);
  const [current, setCurrent] = useState(character.sheet1.classResource.current);
  const [name, setName] = useState(character.sheet1.classResource.name);

  const handleTotalChange = (event) => {
    const newValue = event.target.value;
    setTotal(newValue);
    onCharacterClassResourceTotalChange({ total: newValue });
  };

  const handleCurrentChange = (event) => {
    const newValue = event.target.value;
    setCurrent(newValue);
    onCharacterClassResourceCurrentChange({ current: newValue });
  };

  const handleNameChange = (event) => {
    const newValue = event.target.value;
    setName(newValue);
    onCharacterClassResourceNameChange({ name: newValue });
  };

  //console.log(character.sheet1.classResource.total);
  //console.log(character.sheet1.classResource.current);
  //console.log(character.sheet1.classResource.name);

  return (
    <Card className="border-0 p-0 m-0">
      <Card.Img src={ResourceSVG} />
      <Card.ImgOverlay className="p-0 m-0 d-flex flex-column justify-content-between">
        <Form.Group
          controlId="classResouceTotalAmount"
          className="p-0 px-2 m-0 d-flex flex-row"
        >
          <Form.Label
            className="p-0 m-0 fw-lighter"
          >
            Итого
          </Form.Label>
          <Form.Control
            value={total}
            className="p-0 m-0 text-center border-0 bg-transparent shadow-none"
            onChange={handleTotalChange}
          />
        </Form.Group>
        <Form.Control
          id="classResouceCurrentAmount"
          type="number"
          value={current}
          className="p-0 m-0 flex-grow-1 text-center border-0 bg-transparent shadow-none"
          onChange={handleCurrentChange}
        />
        <Form.Control
          id="classResouceName"
          type="text"
          value={name}
          className="p-0 m-0 fw-bold text-center border-0 bg-transparent shadow-none"
          onChange={handleNameChange}
        />
      </Card.ImgOverlay>
    </Card>
  );
}

export default ClassResource;