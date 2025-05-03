import Card from 'react-bootstrap/Card';
import Form from 'react-bootstrap/Form';
import { FaCog } from 'react-icons/fa';

import ResourceSVG from './assets/Resource.svg';

import { useState } from 'react';
import { useAppDispatch, useAppSelector } from '../../hooks';
import { updateCharacterClassResourceCurrent, updateCharacterClassResourceName, updateCharacterClassResourceResetOn, updateCharacterClassResourceTotal, updateCharacterClassResourceUsePb } from '../../store/sheet1Slice';
import { selectClassResource } from '../../store/selectors/sheet1Selectors';
import { ResourceResetType } from '../../types/state';

const ClassResource: React.FC = () => {
  const dispatch = useAppDispatch();
  const resource = useAppSelector(selectClassResource);

  const [isEditExpanded, setIsEditExpanded] = useState(false);
  const toggleIsEditExpanded = () => setIsEditExpanded(!isEditExpanded);

  const handleTotalChange = (e: React.ChangeEvent<HTMLInputElement>) => {
    const newValue = parseInt(e.target.value, 10);
    if (!isNaN(newValue)) {
      dispatch(updateCharacterClassResourceTotal(newValue));
    }
  };

  const handleCurrentChange = (e: React.ChangeEvent<HTMLInputElement>) => {
    const newValue = parseInt(e.target.value, 10);
    if (!isNaN(newValue)) {
      dispatch(updateCharacterClassResourceCurrent(newValue));
    }
  };

  const handleNameChange = (e: React.ChangeEvent<HTMLInputElement>) => {
    const newValue = e.target.value;
    dispatch(updateCharacterClassResourceName(newValue));
  };

  const handleUsePbChange = (e: React.ChangeEvent<HTMLInputElement>) => {
    const newValue = e.target.checked;
    dispatch(updateCharacterClassResourceUsePb(newValue));
  };

  const handleResetOnChange = (e: React.ChangeEvent<HTMLSelectElement>) => {
    const newValue = e.target.value as ResourceResetType;
    dispatch(updateCharacterClassResourceResetOn(newValue));
  };

  return (
    <Card className="border-0 p-0 m-0">
      <Card.Img src={ResourceSVG} />
      <Card.ImgOverlay className="p-0 px-2 m-0 h-100 d-flex flex-column justify-content-between">
        {isEditExpanded ? (
          <>
            <Form.Group
              controlId="classResource-edit-usePb"
              style={{fontSize: "0.9rem"}}
            >
              <Form.Label as="span">Учитывать бонус мастерства?</Form.Label>
              <Form.Check
                type="checkbox"
                inline
                checked={resource.usePb}
                onChange={handleUsePbChange}
              />
            </Form.Group>
            <Form.Group
              controlId="classResource-edit-resetOn"
              style={{ fontSize: "0.9rem" }}
            >
              <Form.Label as="span">Перезаряжать после</Form.Label>
              <Form.Select
                size="sm"
                value={resource.resetOn}
                onChange={handleResetOnChange}
              >
                <option value="shortRest">Короткий отдых</option>
                <option value="longRest">Долгий отдых</option>
                <option value="-">-</option>
              </Form.Select>
            </Form.Group>
          </>
        ) : (
          <>
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
                  type="number"
                  min={0}
                  step={1}
                  value={resource.total}
                  onChange={handleTotalChange}
                  className="p-0 m-0 text-center border-0 bg-transparent shadow-none"
                />
              </Form.Group>
              <Form.Control
                id="classResouceCurrentAmount"
                type="number"
                min={0}
                max={resource.total}
                step={1}
                value={resource.current}
                onChange={handleCurrentChange}
                className="p-0 m-0 flex-grow-1 text-center border-0 bg-transparent shadow-none"
              />
              <Form.Control
                id="classResouceName"
                type="text"
                value={resource.name}
                className="p-0 m-0 fw-bold text-center border-0 bg-transparent shadow-none"
                onChange={handleNameChange}
              />
          </>
        )}
        <div
          className="position-absolute top-0 d-flex gap-1"
          style={{ right: '0.5rem', cursor: 'pointer' }}
        >
          <FaCog
            onClick={toggleIsEditExpanded}
            aria-label="Редактировать классовый ресурс"
          />
        </div>
      </Card.ImgOverlay>
    </Card>
  );
}

export default ClassResource;