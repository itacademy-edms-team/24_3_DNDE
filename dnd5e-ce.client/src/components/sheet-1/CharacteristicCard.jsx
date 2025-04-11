import Container from 'react-bootstrap/Container';
import Row from 'react-bootstrap/Row';
import Col from 'react-bootstrap/Col';
import Card from 'react-bootstrap/Card';
import Form from 'react-bootstrap/Form';

import CharacteristicCardSVG from './assets/CharacteristicCard.png';
import { useState } from 'react';

function CharacteristicCard({
  classStyles = {
    card: "border-0",
    imgOverlay: "d-flex flex-column justify-content-between h-100 p-0 m-0 border-0"
  },
  characteristic,
  character,
  attrName,
  onCharacteristicChange
}) {


  //console.log(character);
  const [base, setBase] = useState(character.sheet1.characteristics[attrName].base);
  let bonus = Math.floor((base - 10) / 2);

  const handleChange = (event) => {
    const newValue = Number(event.target.value);
    setBase(newValue);
    onCharacteristicChange({ [attrName]: { base: newValue } })
  }

  return (
    <Card className={classStyles.card}>
      <Card.Img src={CharacteristicCardSVG} alt="Card Image" />
      <Card.ImgOverlay className={classStyles.imgOverlay}>
        <Form.Group
          controlId={characteristic.baseControlId}
          className="p-0 px-1 m-0 d-flex flex-column h-100 text-center text-truncate"
        >
          <Form.Label className="text-uppercase fw-bold p-0 m-0">
            {characteristic.labelText}
          </Form.Label>
          <Form.Control
            type="number"
            value={base}
            placeholder="10"
            onChange={handleChange}
            className="p-0 m-0 flex-grow-1 bg-transparent border-0 shadow-none text-center"
            style={{ minHeight: 0 }}  // добавляем minHeight для безопасности
          />
        </Form.Group>

        <Form.Group
          controlId={characteristic.bonusControlId}
          className="p-0 m-0"
        >
          <Form.Control
            value={bonus}
            readOnly
            className="bg-transparent border-0 shadow-none p-0 m-0 mb-md-1 mb-sm-3 text-center"
          />
        </Form.Group>
      </Card.ImgOverlay>
    </Card>
  );
}

export default CharacteristicCard;