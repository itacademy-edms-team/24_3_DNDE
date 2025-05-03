import Card from 'react-bootstrap/Card';
import Form from 'react-bootstrap/Form';
import { FaCog, FaTrash } from 'react-icons/fa';

import ResourceSVG from './assets/Resource.svg';

import { useEffect, useState } from 'react';
import { useAppDispatch, useAppSelector } from '../../hooks';
import { updateCharacterOtherResourceCurrent, updateCharacterOtherResourceName, updateCharacterOtherResourceResetOn, updateCharacterOtherResourceTotal, updateCharacterOtherResourceUsePb } from '../../store/sheet1Slice';
import { selectClassResource } from '../../store/selectors/sheet1Selectors';
import { OtherResourcePropsType, ResourceResetType } from '../../types/state';

const OtherResourceCard: React.FC<OtherResourcePropsType> = ({
  resource,
  isEditMode,
  onDelete
}) => {
  const dispatch = useAppDispatch();

  const [isEditExpanded, setIsEditExpanded] = useState(false);
  const toggleIsEditExpanded = () => setIsEditExpanded(!isEditExpanded);

  useEffect(() => {
    if (!isEditMode) {
      setIsEditExpanded(false);
    }
  }, [isEditMode]);

  const handleTotalChange = (e: React.ChangeEvent<HTMLInputElement>) => {
    const newValue = parseInt(e.target.value, 10);
    if (!isNaN(newValue)) {
      dispatch(updateCharacterOtherResourceTotal({id: resource.id, total: newValue}));
    }
  };

  const handleCurrentChange = (e: React.ChangeEvent<HTMLInputElement>) => {
    const newValue = parseInt(e.target.value, 10);
    if (!isNaN(newValue)) {
      dispatch(updateCharacterOtherResourceCurrent({ id: resource.id, total: newValue }));
    }
  };

  const handleNameChange = (e: React.ChangeEvent<HTMLInputElement>) => {
    const newValue = e.target.value;
    dispatch(updateCharacterOtherResourceName({ id: resource.id, name: newValue }));
  };

  const handleUsePbChange = (e: React.ChangeEvent<HTMLInputElement>) => {
    const newValue = e.target.checked;
    dispatch(updateCharacterOtherResourceUsePb({ id: resource.id, usePb: newValue }));
  };

  const handleResetOnChange = (e: React.ChangeEvent<HTMLSelectElement>) => {
    const newValue = e.target.value as ResourceResetType;
    dispatch(updateCharacterOtherResourceResetOn({ id: resource.id, resetOn: newValue }));
  };

  return (
    <Card className="border-0 p-0 m-0">
      <Card.Img src={ResourceSVG} />
      <Card.ImgOverlay className="p-0 px-2 m-0 h-100 d-flex flex-column justify-content-between">
        {isEditExpanded ? (
          <>
            <Form.Group
              controlId={`otherResource-${resource.id}-edit-usePb` }
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
              controlId={`otherResource-${resource.id}-edit-resetOn`}
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
                controlId={`otherResource-${resource.id}-edit-totalAmount`}
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
                id={`otherResource-${resource.id}-edit-currentAmount`}
                type="number"
                min={0}
                max={resource.total}
                step={1}
                value={resource.current}
                onChange={handleCurrentChange}
                className="p-0 m-0 flex-grow-1 text-center border-0 bg-transparent shadow-none"
              />
              <Form.Control
                id={`otherResource-${resource.id}-edit-name`}
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
          {isEditMode ? (
            <FaCog
              onClick={toggleIsEditExpanded}
              aria-label="Редактировать другой ресурс"
            />
          ) : (
            <FaTrash
              onClick={onDelete}
              aria-label="Удалить ресурс"
            />
          )}
        </div>
      </Card.ImgOverlay>
    </Card>
  );
}

export default OtherResourceCard;